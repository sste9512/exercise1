using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;

namespace StargateAPI.Business.Logging
{
    public static class DatabaseLoggerExtensions
    {
        public static ILoggingBuilder AddDatabaseLogger(this ILoggingBuilder builder)
        {
            builder.Services.AddSingleton<ILoggerProvider, DatabaseLoggerProvider>(serviceProvider =>
            {
                var contextFactory = serviceProvider.GetRequiredService<IDbContextFactory<StargateContext>>();
                return new DatabaseLoggerProvider(contextFactory, serviceProvider);
            });

            return builder;
        }
    }
}
