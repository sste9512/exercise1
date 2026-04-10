using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;

namespace StargateAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public sealed class AuthController(IMediator mediator, ILogger<AuthController> logger) : ControllerBase
    {
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginUser request)
        {
            logger.LogInformation("Login endpoint hit for user: {Username}", request.Username);
            var result = await mediator.Send(request);
            logger.LogInformation("Login completed for user: {Username}", request.Username);
            return result.ToActionResult();
        }

        [HttpPost("signup")]
        [AllowAnonymous]
        public async Task<IActionResult> SignUp([FromBody] SignUpUser request)
        {
            logger.LogInformation("SignUp endpoint hit for user: {Username}", request.Username);
            var result = await mediator.Send(request);
            logger.LogInformation("SignUp completed for user: {Username}", request.Username);
            return result.ToActionResult();
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var result = await mediator.Send(new LogoutUser());
            return result.ToActionResult();
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers()
        {
            var result = await mediator.Send(new GetUsers());
            return result.ToActionResult();
        }

        [HttpPost("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUser request)
        {
            var result = await mediator.Send(request);
            return result.ToActionResult();
        }

        [HttpPut("users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUser request)
        {
            request.Id = id;
            var result = await mediator.Send(request);
            return result.ToActionResult();
        }

        [HttpDelete("users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await mediator.Send(new DeleteUser { Id = id });
            return result.ToActionResult();
        }

        [HttpGet("me")]
        public IActionResult GetMe()
        {
            return Ok(new
            {
                Username = User.Identity?.Name,
                Roles = User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value)
            });
        }
    }
}
