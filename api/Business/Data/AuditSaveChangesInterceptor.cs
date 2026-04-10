using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using StargateAPI.Business.Pipeline;
using System.Text.Json;

namespace StargateAPI.Business.Data
{
    public sealed class AuditSaveChangesInterceptor(IHttpContextAccessor httpContextAccessor) : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is null)
                return base.SavingChangesAsync(eventData, result, cancellationToken);

            var userContext = httpContextAccessor.HttpContext?.RequestServices.GetRequiredService<IUserContext>();

            if (eventData.Context is StargateContext stargateContext && userContext != null)
            {
                stargateContext.SetUserContext(userContext);
                stargateContext.UpdateAuditFields();
            }

            var auditEntries = OnBeforeSaveChanges(eventData.Context);
            
            if (auditEntries.Count > 0)
            {
                eventData.Context.Set<AuditLog>().AddRange(auditEntries);
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            if (eventData.Context is null)
                return base.SavingChanges(eventData, result);

            var userContext = httpContextAccessor.HttpContext?.RequestServices.GetRequiredService<IUserContext>();

            if (eventData.Context is StargateContext stargateContext && userContext != null)
            {
                stargateContext.SetUserContext(userContext);
                stargateContext.UpdateAuditFields();
            }

            var auditEntries = OnBeforeSaveChanges(eventData.Context);
            
            if (auditEntries.Count > 0)
            {
                eventData.Context.Set<AuditLog>().AddRange(auditEntries);
            }

            return base.SavingChanges(eventData, result);
        }

        private List<AuditLog> OnBeforeSaveChanges(DbContext context)
        {
            context.ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditLog>();

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.Entity is AuditLog || entry.Entity is ApplicationLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var auditEntry = CreateAuditEntry(entry);
                if (auditEntry != null)
                {
                    auditEntries.Add(auditEntry);
                }
            }

            return auditEntries;
        }

        private AuditLog? CreateAuditEntry(EntityEntry entry)
        {
            var entityName = entry.Entity.GetType().Name;
            var primaryKey = GetPrimaryKeyValue(entry);

            var auditLog = new AuditLog
            {
                EntityName = entityName,
                EntityId = primaryKey,
                Timestamp = DateTime.UtcNow,
                Action = entry.State.ToString()
            };

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();
            var changedProperties = new List<string>();

            foreach (var property in entry.Properties)
            {
                var propertyName = property.Metadata.Name;

                switch (entry.State)
                {
                    case EntityState.Added:
                        newValues[propertyName] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        oldValues[propertyName] = property.OriginalValue;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            oldValues[propertyName] = property.OriginalValue;
                            newValues[propertyName] = property.CurrentValue;
                            changedProperties.Add(propertyName);
                        }
                        break;
                }
            }

            auditLog.OldValues = oldValues.Count > 0 ? JsonSerializer.Serialize(oldValues) : null;
            auditLog.NewValues = newValues.Count > 0 ? JsonSerializer.Serialize(newValues) : null;
            auditLog.ChangedProperties = changedProperties.Count > 0 ? JsonSerializer.Serialize(changedProperties) : null;

            return auditLog;
        }

        private string GetPrimaryKeyValue(EntityEntry entry)
        {
            var keyProperties = entry.Metadata.FindPrimaryKey()?.Properties;
            if (keyProperties == null || !keyProperties.Any())
                return string.Empty;

            var keyValues = keyProperties
                .Select(p => entry.Property(p.Name).CurrentValue?.ToString() ?? string.Empty);

            return string.Join(",", keyValues);
        }
    }
}
