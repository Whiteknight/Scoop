namespace Scoop.Parsing.Parsers
{
    public struct Result<TOutput> : IParseResult<TOutput>
    {
        private Result(bool success, TOutput value)
        {
            Success = success;
            Value = value;
        }

        public bool Success { get; }
        public TOutput Value { get; }

        public IParseResult<object> Untype() => new Result<object>(Success, Value);

        public static Result<TOutput> Ok(TOutput value) => new Result<TOutput>(true, value);

        public static Result<TOutput> Fail() => new Result<TOutput>(false, default);
    }
}