using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Pipeline;
using StargateAPI.Business.Values;
using Microsoft.Extensions.Logging;

namespace StargateAPI.Business.Pipeline
{
    // Catches any unhandled exception from request handlers and returns a Result.Err(ex)
    // for requests whose response type is Result<TValue, Exception>
    public sealed partial class GenericExceptionHandler<TRequest, TResponse, TException>(
        ILogger<GenericExceptionHandler<TRequest, TResponse, TException>> logger)
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
            var requestName = typeof(TRequest).Name;
            var exceptionType = exception.GetType().Name;
            LogHandlingException(logger, requestName, exceptionType, exception.Message);

            var responseType = typeof(TResponse);
            if (!responseType.IsGenericType || responseType.GetGenericTypeDefinition() != typeof(Result<,>))
                return Task.CompletedTask;
            var errorType = responseType.GetGenericArguments()[1];
            if (!errorType.IsAssignableFrom(typeof(TException))) return Task.CompletedTask;
            // Create Result<TValue, TError>.Err(exception)
            var method = responseType.GetMethod("Err",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (method == null) return Task.CompletedTask;
            var result = method.Invoke(null, [exception]);
            if (result is not TResponse typedResult) return Task.CompletedTask;
            state.SetHandled(typedResult);
            LogExceptionHandled(logger, requestName, exceptionType);

            return Task.CompletedTask;
        }

        [LoggerMessage(LogLevel.Warning,
            "Handling exception for request {RequestName}. Exception type: {ExceptionType}, Message: {Message}")]
        private static partial void LogHandlingException(ILogger logger, string requestName, string exceptionType,
            string message);

        [LoggerMessage(LogLevel.Information,
            "Exception handled and converted to Result.Err for request {RequestName}. Exception type: {ExceptionType}")]
        private static partial void LogExceptionHandled(ILogger logger, string requestName, string exceptionType);
    }
}