using MediatR;
using Polly;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace StargateAPI.Business.Pipeline
{
    public sealed partial class RetryPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IAsyncPolicy<TResponse> _retryPolicy;
        private readonly ILogger<RetryPipelineBehavior<TRequest, TResponse>> _logger;
        private static readonly Random _random = new();

        public RetryPipelineBehavior(ILogger<RetryPipelineBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
            _retryPolicy = Policy<TResponse>
                .Handle<Exception>()
                .WaitAndRetryAsync(3, i => 
                    TimeSpan.FromSeconds(Math.Pow(2, i)) + TimeSpan.FromMilliseconds(_random.Next(0, 1000)));
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            LogExecutingRequest(_logger, requestName);
            
            try
            {
                var result = await _retryPolicy.ExecuteAsync(async () => await next());
                LogRequestCompleted(_logger, requestName);
                return result;
            }
            catch (Exception ex)
            {
                LogRetryExhausted(_logger, requestName, ex);
                throw;
            }
        }

        [LoggerMessage(LogLevel.Debug, "Executing request {RequestName} through retry pipeline")]
        private static partial void LogExecutingRequest(ILogger logger, string requestName);

        [LoggerMessage(LogLevel.Debug, "Request {RequestName} completed successfully")]
        private static partial void LogRequestCompleted(ILogger logger, string requestName);

        [LoggerMessage(LogLevel.Error, "Retry policy exhausted for request {RequestName}")]
        private static partial void LogRetryExhausted(ILogger logger, string requestName, Exception exception);
    }
}
