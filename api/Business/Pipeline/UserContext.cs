namespace StargateAPI.Business.Pipeline
{
    public interface IUserContext
    {
        string? CurrentUser { get; }
    }

    public sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
    {
        public string? CurrentUser => httpContextAccessor.HttpContext?.User?.Identity?.Name;
    }
}
