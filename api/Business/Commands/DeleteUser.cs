using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Values;

namespace StargateAPI.Business.Commands
{
    public sealed class DeleteUser : IRequest<Result<DeleteUserResult, Exception>>
    {
        public int Id { get; set; }
    }

    public sealed class DeleteUserHandler(UserManager<User> userManager, IDbContextFactory<StargateContext> contextFactory)
        : IRequestHandler<DeleteUser, Result<DeleteUserResult, Exception>>
    {
        public async Task<Result<DeleteUserResult, Exception>> Handle(DeleteUser request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await userManager.FindByIdAsync(request.Id.ToString());
                if (user == null)
                {
                    throw new BadHttpRequestException("User not found");
                }

                await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
                context.SoftDeleteUser(user);
                await context.SaveChangesAsync(cancellationToken);

                return Result<DeleteUserResult, Exception>.Ok(new DeleteUserResult { Success = true });
            }
            catch (Exception ex)
            {
                return Result<DeleteUserResult, Exception>.Err(ex);
            }
        }
    }

    public sealed class DeleteUserResult
    {
        public bool Success { get; set; }
    }
}
