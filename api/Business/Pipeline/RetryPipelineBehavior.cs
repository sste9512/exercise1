using MediatR;
using Polly;
using System.Threading;
using System.Threading.Tasks;

namespace StargateAPI.Business.Pipeline
{
    public sealed class RetryPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IAsyncPolicy<TResponse> _retryPolicy;
        private static readonly Random _random = new();

        public RetryPipelineBehavior()
        {
            _retryPolicy = Policy<TResponse>
                .Handle<Exception>()
                .WaitAndRetryAsync(3, i => 
                    TimeSpan.FromSeconds(Math.Pow(2, i)) + TimeSpan.FromMilliseconds(_random.Next(0, 1000)));
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            return await _retryPolicy.ExecuteAsync(async () => await next());
        }
    }
}
