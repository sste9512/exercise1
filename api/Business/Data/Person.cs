using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace StargateAPI.Business.Data
{
    [Table("Person")]
    public class Person : AuditableEntity
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// A person's current astronaut information (one-to-one relationship)
        /// </summary>
        public virtual AstronautDetail? AstronautDetail { get; set; }

        /// <summary>
        /// A person's list of astronaut assignments/duties (one-to-many relationship)
        /// </summary>
        public virtual ICollection<AstronautDuty> AstronautDuties { get; set; } = new HashSet<AstronautDuty>();
    }

    public sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
    {
        public void Configure(EntityTypeBuilder<Person> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);
            
            builder.HasIndex(x => x.Name)
                .IsUnique();
            
            // One-to-one: Person has one AstronautDetail (current astronaut information)
            builder.HasOne(z => z.AstronautDetail)
                .WithOne(z => z.Person)
                .HasForeignKey<AstronautDetail>(z => z.PersonId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // One-to-many: Person has many AstronautDuties (list of assignments)
            builder.HasMany(z => z.AstronautDuties)
                .WithOne(z => z.Person)
                .HasForeignKey(z => z.PersonId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
