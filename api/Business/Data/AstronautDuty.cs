using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace StargateAPI.Business.Data
{
    /// <summary>
    /// Stores a person's astronaut assignment/duty history
    /// One-to-many relationship with Person (a person can have multiple assignments)
    /// </summary>
    [Table("AstronautDuty")]
    public class AstronautDuty : AuditableEntity
    {
        public int Id { get; set; }

        public int PersonId { get; set; }

        public string Rank { get; set; } = string.Empty;

        public string DutyTitle { get; set; } = string.Empty;

        public DateTime DutyStartDate { get; set; }

        public DateTime? DutyEndDate { get; set; }

        public virtual Person Person { get; set; } = null!;
    }

    public class AstronautDutyConfiguration : IEntityTypeConfiguration<AstronautDuty>
    {
        public void Configure(EntityTypeBuilder<AstronautDuty> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            
            builder.Property(x => x.PersonId)
                .IsRequired();
            
            builder.Property(x => x.Rank)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.Property(x => x.DutyTitle)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(x => x.DutyStartDate)
                .IsRequired();
            
            // Index for efficient queries by person
            builder.HasIndex(x => x.PersonId);
            
            // Index for date range queries
            builder.HasIndex(x => new { x.PersonId, x.DutyStartDate });
            
            // Composite index to prevent duplicate assignments
            builder.HasIndex(x => new { x.PersonId, x.DutyTitle, x.DutyStartDate })
                .IsUnique();
        }
    }
}
