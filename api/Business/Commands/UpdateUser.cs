using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Values;

namespace StargateAPI.Business.Commands
{
    public sealed class UpdateUser : IRequest<Result<UpdateUserResult, Exception>>
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
    }

    public sealed class UpdateUserHandler(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
        : IRequestHandler<UpdateUser, Result<UpdateUserResult, Exception>>
    {
        public async Task<Result<UpdateUserResult, Exception>> Handle(UpdateUser request, CancellationToken cancellationToken)
        {
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

                return Result<UpdateUserResult, Exception>.Ok(new UpdateUserResult { Id = user.Id });
            }
            catch (Exception ex)
            {
                return Result<UpdateUserResult, Exception>.Err(ex);
            }
        }
    }

    public sealed class UpdateUserResult
    {
        public int Id { get; set; }
    }
}
