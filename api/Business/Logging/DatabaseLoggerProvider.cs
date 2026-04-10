using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Pipeline;
using System.Collections.Concurrent;

namespace StargateAPI.Business.Logging
{
    [ProviderAlias("Database")]
    public sealed class DatabaseLoggerProvider : ILoggerProvider
    {
        private readonly IDbContextFactory<StargateContext> _contextFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly BlockingCollection<ApplicationLog> _logQueue;
        private readonly Task _writerTask;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public DatabaseLoggerProvider(
            IDbContextFactory<StargateContext> contextFactory,
            IServiceProvider serviceProvider)
        {
            _contextFactory = contextFactory;
            _serviceProvider = serviceProvider;
            _logQueue = new BlockingCollection<ApplicationLog>(1000);
            _cancellationTokenSource = new CancellationTokenSource();
            _writerTask = Task.Run(() => ProcessLogQueue(_cancellationTokenSource.Token));
        }

        public ILogger CreateLogger(string categoryName)
        {
            var userContext = _serviceProvider.GetService<IUserContext>();
            return new DatabaseLogger(categoryName, _contextFactory, userContext, _logQueue);
        }

        private async Task ProcessLogQueue(CancellationToken cancellationToken)
        {
            var batch = new List<ApplicationLog>();
            
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (_logQueue.TryTake(out var log, 1000, cancellationToken))
                    {
                        batch.Add(log);

                        while (batch.Count < 100 && _logQueue.TryTake(out var additionalLog, 10))
                        {
                            batch.Add(additionalLog);
                        }

                        if (batch.Count > 0)
                        {
                            await WriteBatchToDatabase(batch, cancellationToken);
                            batch.Clear();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                }
            }

            if (batch.Count > 0)
            {
                await WriteBatchToDatabase(batch, CancellationToken.None);
            }
        }

        private async Task WriteBatchToDatabase(List<ApplicationLog> logs, CancellationToken cancellationToken)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
                await context.ApplicationLogs.AddRangeAsync(logs, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception)
            {
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _logQueue.CompleteAdding();
            
            try
            {
                _writerTask.Wait(TimeSpan.FromSeconds(5));
            }
            catch (Exception)
            {
            }

            _logQueue.Dispose();
            _cancellationTokenSource.Dispose();
        }
    }
}
