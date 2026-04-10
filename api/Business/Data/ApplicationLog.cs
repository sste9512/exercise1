using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

namespace StargateAPI.Business.Data
{
    [Table("ApplicationLog")]
    public class ApplicationLog
    {
        public long Id { get; set; }

        public DateTime Timestamp { get; set; }

        public string LogLevel { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string? Exception { get; set; }

        public string? EventId { get; set; }

        public string? State { get; set; }

        public string? Username { get; set; }
    }

    public class ApplicationLogConfiguration : IEntityTypeConfiguration<ApplicationLog>
    {
        public void Configure(EntityTypeBuilder<ApplicationLog> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.LogLevel).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Category).HasMaxLength(500).IsRequired();
            builder.Property(x => x.Message).IsRequired();
            builder.HasIndex(x => x.Timestamp);
            builder.HasIndex(x => x.LogLevel);
        }
    }
}
