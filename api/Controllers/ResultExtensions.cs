using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Business.Values;

namespace StargateAPI.Controllers
{
    public static class ResultExtensions
    {
        public static IResult ToHttpResult<TValue>(this Result<TValue, HttpError> result)
        {
            return result.Match<IResult>(
                ok => Results.Ok(ok),
                err => err.StatusCode switch
                {
                    StatusCodes.Status400BadRequest => Results.BadRequest(new { message = err.Message }),
                    StatusCodes.Status401Unauthorized => Results.Unauthorized(),
                    StatusCodes.Status403Forbidden => Results.StatusCode(StatusCodes.Status403Forbidden),
                    StatusCodes.Status404NotFound => Results.NotFound(new { message = err.Message }),
                    StatusCodes.Status409Conflict => Results.Conflict(new { message = err.Message }),
                    StatusCodes.Status422UnprocessableEntity => Results.UnprocessableEntity(new { message = err.Message }),
                    _ => Results.Problem(detail: err.Message, statusCode: err.StatusCode)
                });
        }

        public static IActionResult ToActionResult<TValue>(this Result<TValue, HttpError> result)
        {
            return result.Match<IActionResult>(
                ok => new OkObjectResult(ok),
                err => new ObjectResult(new { message = err.Message })
                {
                    StatusCode = err.StatusCode
                });
        }

        public static IActionResult ToActionResult<TValue>(this Result<TValue, IdentityOperationError> result)
        {
            return result.Match<IActionResult>(
                ok => new OkObjectResult(ok),
                err => new ObjectResult(new { message = err.Message, errors = err.Errors })
                {
                    StatusCode = err.StatusCode
                });
        }

        public static IActionResult ToActionResult<TValue>(this Result<TValue, Exception> result)
        {
            return result.Match<IActionResult>(
                ok => new OkObjectResult(ok),
                err => new ObjectResult(new { message = err.Message })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                });
        }
    }
}
