using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Values;
using Microsoft.Extensions.Logging;

namespace StargateAPI.Business.Commands
{
    public sealed class CreateUser : IRequest<Result<CreateUserResult, Exception>>
    {
        public required string Username { get; set; } = string.Empty;
        public required string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
    }

    public sealed partial class CreateUserHandler(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager, ILogger<CreateUserHandler> logger)
        : IRequestHandler<CreateUser, Result<CreateUserResult, Exception>>
    {
        public async Task<Result<CreateUserResult, Exception>> Handle(CreateUser request, CancellationToken cancellationToken)
        {
            LogExecutingCreateUser(logger, request.Username, request.Role);
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
                    Email = $"{request.Username}@stargate.com",
                };

                var result = await userManager.CreateAsync(newUser, request.Password);
                if (!result.Succeeded)
                {
                    throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                if (!await roleManager.RoleExistsAsync(request.Role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int> { Name = request.Role });
                }

                await userManager.AddToRoleAsync(newUser, request.Role);

                LogCreateUserSuccess(logger, request.Username, request.Role, newUser.Id);
                return Result<CreateUserResult, Exception>.Ok(new CreateUserResult { Id = newUser.Id });
            }
            catch (Exception ex)
            {
                LogCreateUserError(logger, request.Username, ex);
                return Result<CreateUserResult, Exception>.Err(ex);
            }
        }

        [LoggerMessage(LogLevel.Information, "Executing CreateUser command for username: {Username}, role: {Role}")]
        private static partial void LogExecutingCreateUser(ILogger logger, string username, string role);

        [LoggerMessage(LogLevel.Information, "CreateUser command completed successfully for username: {Username}, role: {Role}. Created user with ID: {Id}")]
        private static partial void LogCreateUserSuccess(ILogger logger, string username, string role, int id);

        [LoggerMessage(LogLevel.Error, "Error executing CreateUser command for username: {Username}")]
        private static partial void LogCreateUserError(ILogger logger, string username, Exception exception);
    }

    public sealed class CreateUserResult
    {
        public int Id { get; set; }
    }
}
