using MediatR;
using Microsoft.AspNetCore.Identity;
using StargateAPI.Business.Data;
using StargateAPI.Business.Values;
using Microsoft.Extensions.Logging;

namespace StargateAPI.Business.Commands
{
    public sealed class LoginUser : IRequest<Result<LoginUserResult, Exception>>
    {
        public required string Username { get; set; } = string.Empty;
        public required string Password { get; set; } = string.Empty;
    }

    public sealed partial class LoginUserHandler(UserManager<User> userManager, SignInManager<User> signInManager, ILogger<LoginUserHandler> logger)
        : IRequestHandler<LoginUser, Result<LoginUserResult, Exception>>
    {
        public async Task<Result<LoginUserResult, Exception>> Handle(LoginUser request, CancellationToken cancellationToken)
        {
            LogExecutingLogin(logger, request.Username);
            try
            {
                var user = await userManager.FindByNameAsync(request.Username);
                if (user == null)
                {
                    throw new BadHttpRequestException("Invalid username or password", StatusCodes.Status401Unauthorized);
                }

                var result = await signInManager.PasswordSignInAsync(user, request.Password, isPersistent: false, lockoutOnFailure: false);
                if (!result.Succeeded)
                {
                    throw new BadHttpRequestException("Invalid username or password", StatusCodes.Status401Unauthorized);
                }

                var roles = await userManager.GetRolesAsync(user);

                LogLoginSuccess(logger, request.Username);
                return Result<LoginUserResult, Exception>.Ok(new LoginUserResult
                {
                    Username = user.UserName!,
                    Roles = roles.ToList()
                });
            }
            catch (Exception ex)
            {
                LogLoginError(logger, request.Username, ex);
                return Result<LoginUserResult, Exception>.Err(ex);
            }
        }

        [LoggerMessage(LogLevel.Information, "Executing Login command for username: {Username}")]
        private static partial void LogExecutingLogin(ILogger logger, string username);

        [LoggerMessage(LogLevel.Information, "Login command completed successfully for username: {Username}")]
        private static partial void LogLoginSuccess(ILogger logger, string username);

        [LoggerMessage(LogLevel.Warning, "Login failed for username: {Username}")]
        private static partial void LogLoginError(ILogger logger, string username, Exception exception);
    }

    public sealed class LoginUserResult
    {
        public string Username { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }
}
