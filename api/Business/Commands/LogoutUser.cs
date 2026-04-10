using MediatR;
using Microsoft.AspNetCore.Identity;
using StargateAPI.Business.Data;
using StargateAPI.Business.Values;

namespace StargateAPI.Business.Commands
{
    public sealed class LogoutUser : IRequest<Result<bool, Exception>>
    {
    }

    public sealed class LogoutUserHandler(SignInManager<User> signInManager)
        : IRequestHandler<LogoutUser, Result<bool, Exception>>
    {
        public async Task<Result<bool, Exception>> Handle(LogoutUser request, CancellationToken cancellationToken)
        {
            try
            {
                await signInManager.SignOutAsync();
                return Result<bool, Exception>.Ok(true);
            }
            catch (Exception ex)
            {
                return Result<bool, Exception>.Err(ex);
            }
        }
    }
}
