using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Pipeline;
using System.Data;

namespace StargateAPI.Business.Data
{
    public sealed class StargateContext(DbContextOptions<StargateContext> options, IUserContext? userContext = null)
        : IdentityDbContext<User, IdentityRole<int>, int>(options)
    {
        public IDbConnection Connection => Database.GetDbConnection();
        public DbSet<Person> People => Set<Person>();
        public DbSet<AstronautDetail> AstronautDetails => Set<AstronautDetail>();
        public DbSet<AstronautDuty> AstronautDuties => Set<AstronautDuty>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<ApplicationLog> ApplicationLogs => Set<ApplicationLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(StargateContext).Assembly);

            // Apply soft delete query filters to all entities inheriting from AuditableEntity
            modelBuilder.Entity<Person>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<AstronautDetail>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<AstronautDuty>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);

            SeedData(modelBuilder);
        }

        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateAuditFields()
        {
            var auditableEntries = ChangeTracker.Entries<AuditableEntity>();

            foreach (var entry in auditableEntries)
            {
                var currentUser = userContext?.CurrentUser ?? "System";
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.CreatedBy = currentUser;
                        entry.Entity.IsDeleted = false;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        entry.Entity.UpdatedBy = currentUser;
                        break;
                }
            }

            var userEntries = ChangeTracker.Entries<User>();
            foreach (var entry in userEntries)
            {
                var currentUser = userContext?.CurrentUser ?? "System";
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.CreatedBy = currentUser;
                        entry.Entity.IsDeleted = false;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        entry.Entity.UpdatedBy = currentUser;
                        break;
                }
            }
        }

        public void SoftDelete<T>(T entity) where T : AuditableEntity
        {
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            entity.DeletedBy = userContext?.CurrentUser ?? "System";
            Entry(entity).State = EntityState.Modified;
        }

        public void SoftDeleteUser(User user)
        {
            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.DeletedBy = userContext?.CurrentUser ?? "System";
            Entry(user).State = EntityState.Modified;
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Roles
            modelBuilder.Entity<IdentityRole<int>>().HasData(
                new IdentityRole<int> { Id = 1, Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole<int> { Id = 2, Name = "User", NormalizedName = "USER" }
            );

            // Initial Admin User
            var hasher = new PasswordHasher<User>();
            var adminUser = new User
            {
                Id = 1,
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@stargate.com",
                NormalizedEmail = "ADMIN@STARGATE.COM",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                IsDeleted = false,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "password");

            modelBuilder.Entity<User>().HasData(adminUser);

            // Seed UserRole
            modelBuilder.Entity<IdentityUserRole<int>>().HasData(
                new IdentityUserRole<int> { UserId = 1, RoleId = 1 }
            );

            //add seed data
            modelBuilder.Entity<Person>()
                .HasData(
                    new Person
                    {
                        Id = 1,
                        Name = "John Doe"
                    },
                    new Person
                    {
                        Id = 2,
                        Name = "Jane Doe"
                    }
                );

            modelBuilder.Entity<AstronautDetail>()
                .HasData(
                    new AstronautDetail
                    {
                        Id = 1,
                        PersonId = 1,
                        CurrentRank = "1LT",
                        CurrentDutyTitle = "Commander",
                        CareerStartDate = DateTime.Now
                    }
                );

            modelBuilder.Entity<AstronautDuty>()
                .HasData(
                    new AstronautDuty
                    {
                        Id = 1,
                        PersonId = 1,
                        DutyStartDate = DateTime.Now,
                        DutyTitle = "Commander",
                        Rank = "1LT"
                    }
                );
        }
    }
}
