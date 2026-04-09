using MediatR;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;
using System.Net;

namespace StargateAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class AstronautDutyController(IMediator mediator) : ControllerBase
    {
        [HttpGet("{name}")]
        public async Task<IActionResult> GetAstronautDutiesByName(string name)
        {
            var result = await mediator.Send(new GetAstronautDutiesByName()
            {
                Name = name
            });
            return result.ToActionResult();
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateAstronautDuty([FromBody] CreateAstronautDuty request)
        {
            var result = await mediator.Send(request);
            return result.ToActionResult();
        }
    }
}