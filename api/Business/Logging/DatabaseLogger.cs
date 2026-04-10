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
        private readonly IUserContext? _userContext;
        private readonly BlockingCollection<ApplicationLog> _logQueue;

        public DatabaseLogger(
            string categoryName,
            IDbContextFactory<StargateContext> contextFactory,
            IUserContext? userContext,
            BlockingCollection<ApplicationLog> logQueue)
        {
            _categoryName = categoryName;
            _contextFactory = contextFactory;
            _userContext = userContext;
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

            var log = new ApplicationLog
            {
                Timestamp = DateTime.UtcNow,
                LogLevel = logLevel.ToString(),
                Category = _categoryName,
                Message = formatter(state, exception),
                Exception = exception?.ToString(),
                EventId = eventId.Id != 0 ? eventId.ToString() : null,
                State = state?.ToString(),
                Username = _userContext?.CurrentUser
            };

            _logQueue.Add(log);
        }
    }
}
