using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Values;
using Microsoft.Extensions.Logging;

namespace StargateAPI.Business.Queries
{
    public sealed class GetUsers : IRequest<Result<GetUsersResult, Exception>>
    {
    }

    public sealed partial class GetUsersHandler(UserManager<User> userManager, ILogger<GetUsersHandler> logger)
        : IRequestHandler<GetUsers, Result<GetUsersResult, Exception>>
    {
        public async Task<Result<GetUsersResult, Exception>> Handle(GetUsers request,
            CancellationToken cancellationToken)
        {
            LogExecutingGetUsers(logger);
            try
            {
                var users = await userManager.Users
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        Username = u.UserName ?? string.Empty,
                    })
                    .ToListAsync(cancellationToken);

                foreach (var userDto in users)
                {
                    var user = await userManager.FindByIdAsync(userDto.Id.ToString());
                    if (user != null)
                    {
                        var roles = await userManager.GetRolesAsync(user);
                        userDto.Role = string.Join(", ", roles);
                    }
                }

                LogGetUsersSuccess(logger, users.Count);
                return Result<GetUsersResult, Exception>.Ok(new GetUsersResult { Users = users });
            }
            catch (Exception ex)
            {
                LogGetUsersError(logger, ex);
                return Result<GetUsersResult, Exception>.Err(ex);
            }
        }

        [LoggerMessage(LogLevel.Information, "Executing GetUsers query")]
        private static partial void LogExecutingGetUsers(ILogger logger);

        [LoggerMessage(LogLevel.Information, "GetUsers query completed successfully. Retrieved {Count} users")]
        private static partial void LogGetUsersSuccess(ILogger logger, int count);

        [LoggerMessage(LogLevel.Error, "Error executing GetUsers query")]
        private static partial void LogGetUsersError(ILogger logger, Exception exception);
    }

    public sealed class GetUsersResult
    {
        public List<UserDto> Users { get; set; } = new();
    }

    public sealed class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}