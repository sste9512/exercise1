using MediatR;
using Microsoft.AspNetCore.Identity;
using StargateAPI.Business.Data;
using StargateAPI.Business.Values;
using Microsoft.Extensions.Logging;

namespace StargateAPI.Business.Commands
{
    public sealed class LogoutUser : IRequest<Result<bool, Exception>>
    {
    }

    public sealed partial class LogoutUserHandler(SignInManager<User> signInManager, ILogger<LogoutUserHandler> logger)
        : IRequestHandler<LogoutUser, Result<bool, Exception>>
    {
        public async Task<Result<bool, Exception>> Handle(LogoutUser request, CancellationToken cancellationToken)
        {
            LogExecutingLogout(logger);
            try
            {
                await signInManager.SignOutAsync();
                LogLogoutSuccess(logger);
                return Result<bool, Exception>.Ok(true);
            }
            catch (Exception ex)
            {
                LogLogoutError(logger, ex);
                return Result<bool, Exception>.Err(ex);
            }
        }

        [LoggerMessage(LogLevel.Information, "Executing Logout command")]
        private static partial void LogExecutingLogout(ILogger logger);

        [LoggerMessage(LogLevel.Information, "Logout command completed successfully")]
        private static partial void LogLogoutSuccess(ILogger logger);

        [LoggerMessage(LogLevel.Error, "Error executing Logout command")]
        private static partial void LogLogoutError(ILogger logger, Exception exception);
    }
}
