using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Values;
using Microsoft.Extensions.Logging;

namespace StargateAPI.Business.Commands
{
    public sealed class DeleteUser : IRequest<Result<DeleteUserResult, Exception>>
    {
        public int Id { get; set; }
    }

    public sealed partial class DeleteUserHandler(UserManager<User> userManager, IDbContextFactory<StargateContext> contextFactory, ILogger<DeleteUserHandler> logger)
        : IRequestHandler<DeleteUser, Result<DeleteUserResult, Exception>>
    {
        public async Task<Result<DeleteUserResult, Exception>> Handle(DeleteUser request, CancellationToken cancellationToken)
        {
            LogExecutingDeleteUser(logger, request.Id);
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

                LogDeleteUserSuccess(logger, request.Id);
                return Result<DeleteUserResult, Exception>.Ok(new DeleteUserResult { Success = true });
            }
            catch (Exception ex)
            {
                LogDeleteUserError(logger, request.Id, ex);
                return Result<DeleteUserResult, Exception>.Err(ex);
            }
        }

        [LoggerMessage(LogLevel.Information, "Executing DeleteUser command for user ID: {UserId}")]
        private static partial void LogExecutingDeleteUser(ILogger logger, int userId);

        [LoggerMessage(LogLevel.Information, "DeleteUser command completed successfully for user ID: {UserId}")]
        private static partial void LogDeleteUserSuccess(ILogger logger, int userId);

        [LoggerMessage(LogLevel.Error, "Error executing DeleteUser command for user ID: {UserId}")]
        private static partial void LogDeleteUserError(ILogger logger, int userId, Exception exception);
    }

    public sealed class DeleteUserResult
    {
        public bool Success { get; set; }
    }
}
