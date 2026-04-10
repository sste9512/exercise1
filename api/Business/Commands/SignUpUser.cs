using MediatR;
using Microsoft.AspNetCore.Identity;
using StargateAPI.Business.Data;
using StargateAPI.Business.Values;
using Microsoft.Extensions.Logging;

namespace StargateAPI.Business.Commands
{
    public sealed class SignUpUser : IRequest<Result<SignUpUserResult, Exception>>
    {
        public required string Username { get; set; } = string.Empty;
        public required string Password { get; set; } = string.Empty;
        public string? Email { get; set; }
    }

    public sealed partial class SignUpUserHandler(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole<int>> roleManager, ILogger<SignUpUserHandler> logger)
        : IRequestHandler<SignUpUser, Result<SignUpUserResult, Exception>>
    {
        public async Task<Result<SignUpUserResult, Exception>> Handle(SignUpUser request, CancellationToken cancellationToken)
        {
            LogExecutingSignUp(logger, request.Username);
            try
            {
                var existingUser = await userManager.FindByNameAsync(request.Username);
                if (existingUser != null)
                {
                    throw new BadHttpRequestException("Username already exists");
                }

                var newUser = new User
                {
                    UserName = request.Username,
                    Email = request.Email ?? $"{request.Username}@stargate.com",
                };

                var result = await userManager.CreateAsync(newUser, request.Password);
                if (!result.Succeeded)
                {
                    throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                const string defaultRole = "User";
                if (!await roleManager.RoleExistsAsync(defaultRole))
                {
                    await roleManager.CreateAsync(new IdentityRole<int> { Name = defaultRole });
                }

                await userManager.AddToRoleAsync(newUser, defaultRole);

                await signInManager.SignInAsync(newUser, isPersistent: false);

                var roles = await userManager.GetRolesAsync(newUser);

                LogSignUpSuccess(logger, request.Username, newUser.Id);
                return Result<SignUpUserResult, Exception>.Ok(new SignUpUserResult
                {
                    Id = newUser.Id,
                    Username = newUser.UserName!,
                    Roles = roles.ToList()
                });
            }
            catch (Exception ex)
            {
                LogSignUpError(logger, request.Username, ex);
                return Result<SignUpUserResult, Exception>.Err(ex);
            }
        }

        [LoggerMessage(LogLevel.Information, "Executing SignUp command for username: {Username}")]
        private static partial void LogExecutingSignUp(ILogger logger, string username);

        [LoggerMessage(LogLevel.Information, "SignUp command completed successfully for username: {Username}. Created user with ID: {Id}")]
        private static partial void LogSignUpSuccess(ILogger logger, string username, int id);

        [LoggerMessage(LogLevel.Error, "Error executing SignUp command for username: {Username}")]
        private static partial void LogSignUpError(ILogger logger, string username, Exception exception);
    }

    public sealed class SignUpUserResult
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }
}
