using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;
using System.Net;

namespace StargateAPI.Controllers
{
   
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public sealed class PersonController(IMediator mediator) : ControllerBase
    {
        [HttpGet("")]
        public async Task<IActionResult> GetPeople()
        {
            var result = await mediator.Send(new GetPeople());
            return result.ToActionResult();
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetPersonByName(string name)
        {
            var result = await mediator.Send(new GetPersonByName()
            {
                Name = name
            });
            return result.ToActionResult();
        }

        [HttpPost("")]
        public async Task<IActionResult> CreatePerson([FromBody] string name)
        {
            var result = await mediator.Send(new CreatePerson()
            {
                Name = name
            });
            return result.ToActionResult();
        }
    }
}