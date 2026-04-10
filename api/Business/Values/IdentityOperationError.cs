using Microsoft.AspNetCore.Identity;

namespace StargateAPI.Business.Values
{
    public sealed record IdentityOperationError(int StatusCode, string Message, IEnumerable<IdentityError>? Errors = null);
}
