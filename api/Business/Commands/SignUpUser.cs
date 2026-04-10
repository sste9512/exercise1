using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Values;
using Microsoft.Extensions.Logging;

namespace StargateAPI.Business.Commands
{
    public sealed class SignUpUser : IRequest<Result<SignUpUserResult, IdentityOperationError>>
    {
        public required string Username { get; set; } = string.Empty;
        public required string Password { get; set; } = string.Empty;
        public string? Email { get; set; }
    }

    public sealed partial class SignUpUserHandler(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager, ILogger<SignUpUserHandler> logger, IDbContextFactory<StargateContext> contextFactory)
        : IRequestHandler<SignUpUser, Result<SignUpUserResult, IdentityOperationError>>
    {
        public async Task<Result<SignUpUserResult, IdentityOperationError>> Handle(SignUpUser request, CancellationToken cancellationToken)
        {
            LogExecutingSignUp(logger, request.Username);

            try
            {
                var existingUser = await userManager.FindByNameAsync(request.Username);
                if (existingUser != null)
                {
                    logger.LogWarning("SignUp failed: Username {Username} already exists", request.Username);
                    return Result<SignUpUserResult, IdentityOperationError>.Err(new IdentityOperationError(StatusCodes.Status400BadRequest, "Username already exists"));
                }

                logger.LogDebug("Creating new user with username: {Username}", request.Username);
                var newUser = new User
                {
                    UserName = request.Username,
                    Email = request.Email ?? $"{request.Username}@stargate.com",
                };

                logger.LogDebug("Attempting to create user in database");
                var result = await userManager.CreateAsync(newUser, request.Password);
                if (!result.Succeeded)
                {
                    logger.LogWarning("User creation failed for {Username}: {Errors}", request.Username, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return Result<SignUpUserResult, IdentityOperationError>.Err(new IdentityOperationError(StatusCodes.Status400BadRequest, "User creation failed", result.Errors));
                }
                logger.LogDebug("User {Username} created successfully with ID: {Id}", request.Username, newUser.Id);

                const string defaultRole = "User";
                logger.LogDebug("Checking if role {Role} exists", defaultRole);
                if (!await roleManager.RoleExistsAsync(defaultRole))
                {
                    logger.LogDebug("Role {Role} does not exist, creating it", defaultRole);
                    var roleResult = await roleManager.CreateAsync(new IdentityRole<int> { Name = defaultRole });
                    if (!roleResult.Succeeded)
                    {
                        logger.LogError("Role creation failed for {Role}: {Errors}", defaultRole, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                        return Result<SignUpUserResult, IdentityOperationError>.Err(new IdentityOperationError(StatusCodes.Status500InternalServerError, "Role creation failed", roleResult.Errors));
                    }
                }

                logger.LogDebug("Adding user {Username} to role {Role}", request.Username, defaultRole);
                var addToRoleResult = await userManager.AddToRoleAsync(newUser, defaultRole);
                if (!addToRoleResult.Succeeded)
                {
                    logger.LogError("Adding user {Username} to role {Role} failed: {Errors}", request.Username, defaultRole, string.Join(", ", addToRoleResult.Errors.Select(e => e.Description)));
                    return Result<SignUpUserResult, IdentityOperationError>.Err(new IdentityOperationError(StatusCodes.Status500InternalServerError, "Adding user to role failed", addToRoleResult.Errors));
                }

                logger.LogDebug("Retrieving roles for user {Username}", request.Username);
                var roles = await userManager.GetRolesAsync(newUser);

                LogSignUpSuccess(logger, request.Username, newUser.Id);
                return Result<SignUpUserResult, IdentityOperationError>.Ok(new SignUpUserResult
                {
                    Id = newUser.Id,
                    Username = newUser.UserName!,
                    Roles = roles.ToList()
                });
            }
            catch (Exception ex)
            {
                LogSignUpError(logger, request.Username, ex);
                return Result<SignUpUserResult, IdentityOperationError>.Err(new IdentityOperationError(StatusCodes.Status500InternalServerError, ex.Message));
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
