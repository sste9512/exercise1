using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Pipeline;
using StargateAPI.Business.Values;

namespace StargateAPI.Business.Pipeline
{
    // Catches any unhandled exception from request handlers and returns a Result.Err(ex)
    // for requests whose response type is Result<TValue, Exception>
    public sealed class GenericExceptionHandler<TRequest, TResponse, TException>
        : IRequestExceptionHandler<TRequest, TResponse, TException>
        where TResponse : class
        where TException : Exception
    {
        public Task Handle(
            TRequest request,
            TException exception,
            RequestExceptionHandlerState<TResponse> state,
            CancellationToken cancellationToken)
        {
            var responseType = typeof(TResponse);
            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<,>))
            {
                var errorType = responseType.GetGenericArguments()[1];
                if (errorType.IsAssignableFrom(typeof(TException)))
                {
                    // Create Result<TValue, TError>.Err(exception)
                    var method = responseType.GetMethod("Err", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (method != null)
                    {
                        var result = method.Invoke(null, [exception]);
                        if (result is TResponse typedResult)
                        {
                            state.SetHandled(typedResult);
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
