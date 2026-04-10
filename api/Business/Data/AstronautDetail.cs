using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace StargateAPI.Business.Data
{
    /// <summary>
    /// Stores a person's CURRENT astronaut information
    /// One-to-one relationship with Person
    /// </summary>
    [Table("AstronautDetail")]
    public class AstronautDetail : AuditableEntity
    {
        public int Id { get; set; }

        public int PersonId { get; set; }

        public string CurrentRank { get; set; } = string.Empty;

        public string CurrentDutyTitle { get; set; } = string.Empty;

        public DateTime CareerStartDate { get; set; }

        public DateTime? CareerEndDate { get; set; }

        public virtual Person Person { get; set; } = null!;
    }

    public class AstronautDetailConfiguration : IEntityTypeConfiguration<AstronautDetail>
    {
        public void Configure(EntityTypeBuilder<AstronautDetail> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            
            // Enforce one-to-one: Each person can have only ONE current astronaut detail
            builder.HasIndex(x => x.PersonId)
                .IsUnique();
            
            builder.Property(x => x.PersonId)
                .IsRequired();
            
            builder.Property(x => x.CurrentRank)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.Property(x => x.CurrentDutyTitle)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(x => x.CareerStartDate)
                .IsRequired();
        }
    }
}
