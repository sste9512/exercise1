namespace StargateAPI.Business.Values
{
    public readonly record struct HttpError(int StatusCode, string Message);
}
