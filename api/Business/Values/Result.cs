namespace StargateAPI.Business.Values
{
    public readonly struct Result<TValue, TError> 
    {
        private readonly TValue? _value;
        private readonly TError? _error;

        private Result(TValue value)
        {
            _value = value;
            _error = default;
            IsOk = true;
        }

        private Result(TError error)
        {
            _value = default;
            _error = error;
            IsOk = false;
        }

        public bool IsOk { get; }

        public bool IsErr => !IsOk;

        public TValue Value => IsOk
            ? _value!
            : throw new InvalidOperationException("Cannot access Value when Result is Err.");

        public TError Error => IsErr
            ? _error!
            : throw new InvalidOperationException("Cannot access Error when Result is Ok.");

        public static Result<TValue, TError> Ok(TValue value) => new(value);

        public static Result<TValue, TError> Err(TError error) => new(error);

        public TResult Match<TResult>(Func<TValue, TResult> ok, Func<TError, TResult> err)
        {
            return IsOk ? ok(Value) : err(Error);
        }
    }
}
