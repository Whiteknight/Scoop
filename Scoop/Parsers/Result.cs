namespace Scoop.Parsers
{
    public struct Result<TOutput> : IParseResult<TOutput>
    {
        public Result(bool success, TOutput value)
        {
            Success = success;
            Value = value;
        }

        public bool Success { get; }
        public TOutput Value { get; }

        public static Result<TOutput> Fail() => new Result<TOutput>(false, default);
    }
}