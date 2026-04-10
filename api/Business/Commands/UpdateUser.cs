using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Values;
using Microsoft.Extensions.Logging;

namespace StargateAPI.Business.Commands
{
    public sealed class UpdateUser : IRequest<Result<UpdateUserResult, Exception>>
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
    }

    public sealed partial class UpdateUserHandler(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager, ILogger<UpdateUserHandler> logger)
        : IRequestHandler<UpdateUser, Result<UpdateUserResult, Exception>>
    {
        public async Task<Result<UpdateUserResult, Exception>> Handle(UpdateUser request, CancellationToken cancellationToken)
        {
            LogExecutingUpdateUser(logger, request.Id);
            try
            {
                var user = await userManager.FindByIdAsync(request.Id.ToString());
                if (user == null)
                {
                    throw new BadHttpRequestException("User not found");
                }

                if (!string.IsNullOrEmpty(request.Username))
                {
                    var existingUser = await userManager.FindByNameAsync(request.Username);
                    if (existingUser != null && existingUser.Id != user.Id)
                    {
                        throw new BadHttpRequestException("Username already exists");
                    }
                    user.UserName = request.Username;
                }

                if (!string.IsNullOrEmpty(request.Password))
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await userManager.ResetPasswordAsync(user, token, request.Password);
                    if (!result.Succeeded)
                    {
                        throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }

                if (!string.IsNullOrEmpty(request.Role))
                {
                    var currentRoles = await userManager.GetRolesAsync(user);
                    await userManager.RemoveFromRolesAsync(user, currentRoles);
                    
                    if (!await roleManager.RoleExistsAsync(request.Role))
                    {
                        await roleManager.CreateAsync(new IdentityRole<int> { Name = request.Role });
                    }
                    await userManager.AddToRoleAsync(user, request.Role);
                }

                await userManager.UpdateAsync(user);

                LogUpdateUserSuccess(logger, request.Id, user.UserName ?? "unknown");
                return Result<UpdateUserResult, Exception>.Ok(new UpdateUserResult { Id = user.Id });
            }
            catch (Exception ex)
            {
                LogUpdateUserError(logger, request.Id, ex);
                return Result<UpdateUserResult, Exception>.Err(ex);
            }
        }

        [LoggerMessage(LogLevel.Information, "Executing UpdateUser command for user ID: {UserId}")]
        private static partial void LogExecutingUpdateUser(ILogger logger, int userId);

        [LoggerMessage(LogLevel.Information, "UpdateUser command completed successfully for user ID: {UserId}, username: {Username}")]
        private static partial void LogUpdateUserSuccess(ILogger logger, int userId, string username);

        [LoggerMessage(LogLevel.Error, "Error executing UpdateUser command for user ID: {UserId}")]
        private static partial void LogUpdateUserError(ILogger logger, int userId, Exception exception);
    }

    public sealed class UpdateUserResult
    {
        public int Id { get; set; }
    }
}
