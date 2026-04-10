using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Pipeline;
using System.Collections.Concurrent;

namespace StargateAPI.Business.Logging
{
    public sealed class DatabaseLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly IDbContextFactory<StargateContext> _contextFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly BlockingCollection<ApplicationLog> _logQueue;

        public DatabaseLogger(
            string categoryName,
            IDbContextFactory<StargateContext> contextFactory,
            IServiceProvider serviceProvider,
            BlockingCollection<ApplicationLog> logQueue)
        {
            _categoryName = categoryName;
            _contextFactory = contextFactory;
            _serviceProvider = serviceProvider;
            _logQueue = logQueue;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            // Try to get current user from scoped service provider
            string? username = null;
            try
            {
                var userContext = _serviceProvider.GetService<IUserContext>();
                username = userContext?.CurrentUser;
            }
            catch
            {
                // If we can't resolve IUserContext (e.g., outside of a request scope), username remains null
            }

            var log = new ApplicationLog
            {
                Timestamp = DateTime.UtcNow,
                LogLevel = logLevel.ToString(),
                Category = _categoryName,
                Message = formatter(state, exception),
                Exception = exception?.ToString(),
                EventId = eventId.Id != 0 ? eventId.ToString() : null,
                State = state?.ToString(),
                Username = username
            };

            _logQueue.Add(log);
        }
    }
}
