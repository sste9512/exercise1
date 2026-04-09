using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

namespace StargateAPI.Business.Data
{
    [Table("AuditLog")]
    public class AuditLog
    {
        public int Id { get; set; }

        public string EntityName { get; set; } = string.Empty;

        public string EntityId { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public string? OldValues { get; set; }

        public string? NewValues { get; set; }

        public string? ChangedProperties { get; set; }

        public DateTime Timestamp { get; set; }

        public string? UserId { get; set; }
    }

    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.EntityName).HasMaxLength(256);
            builder.Property(x => x.EntityId).HasMaxLength(256);
            builder.Property(x => x.Action).HasMaxLength(50);
        }
    }
}
