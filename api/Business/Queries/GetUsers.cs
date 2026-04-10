using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Values;

namespace StargateAPI.Business.Queries
{
    public sealed class GetUsers : IRequest<Result<GetUsersResult, Exception>>
    {
    }

    public sealed class GetUsersHandler(UserManager<User> userManager)
        : IRequestHandler<GetUsers, Result<GetUsersResult, Exception>>
    {
        public async Task<Result<GetUsersResult, Exception>> Handle(GetUsers request, CancellationToken cancellationToken)
        {
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

                return Result<GetUsersResult, Exception>.Ok(new GetUsersResult { Users = users });
            }
            catch (Exception ex)
            {
                return Result<GetUsersResult, Exception>.Err(ex);
            }
        }
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
