using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Values;

namespace StargateAPI.Business.Commands
{
    public sealed class CreateUser : IRequest<Result<CreateUserResult, Exception>>
    {
        public required string Username { get; set; } = string.Empty;
        public required string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
    }

    public sealed class CreateUserHandler(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
        : IRequestHandler<CreateUser, Result<CreateUserResult, Exception>>
    {
        public async Task<Result<CreateUserResult, Exception>> Handle(CreateUser request, CancellationToken cancellationToken)
        {
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

                return Result<CreateUserResult, Exception>.Ok(new CreateUserResult { Id = newUser.Id });
            }
            catch (Exception ex)
            {
                return Result<CreateUserResult, Exception>.Err(ex);
            }
        }
    }

    public sealed class CreateUserResult
    {
        public int Id { get; set; }
    }
}
