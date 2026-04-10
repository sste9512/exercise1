using MediatR;
using Microsoft.AspNetCore.Identity;
using StargateAPI.Business.Data;
using StargateAPI.Business.Values;

namespace StargateAPI.Business.Commands
{
    public sealed class LoginUser : IRequest<Result<LoginUserResult, Exception>>
    {
        public required string Username { get; set; } = string.Empty;
        public required string Password { get; set; } = string.Empty;
    }

    public sealed class LoginUserHandler(UserManager<User> userManager, SignInManager<User> signInManager)
        : IRequestHandler<LoginUser, Result<LoginUserResult, Exception>>
    {
        public async Task<Result<LoginUserResult, Exception>> Handle(LoginUser request, CancellationToken cancellationToken)
        {
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

                return Result<LoginUserResult, Exception>.Ok(new LoginUserResult
                {
                    Username = user.UserName!,
                    Roles = roles.ToList()
                });
            }
            catch (Exception ex)
            {
                return Result<LoginUserResult, Exception>.Err(ex);
            }
        }
    }

    public sealed class LoginUserResult
    {
        public string Username { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }
}
