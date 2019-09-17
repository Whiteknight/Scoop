using System;

namespace Scoop
{
    public struct ParseResult<TOutput>
    {
        public bool IsSuccess { get; }
        public TOutput Value { get; }
        public Exception Exception { get; }
        public string Name { get; }

        public ParseResult(bool success, TOutput value, string name, Exception e)
        {
            IsSuccess = success;
            Value = value;
            Name = name;
            Exception = e;
        }

        public static ParseResult<TOutput> Success(TOutput value, string name)
        {
            return new ParseResult<TOutput>(true, value, name, null);
        }

        public static ParseResult<TOutput> Fail(string name)
        {
            return new ParseResult<TOutput>(false, default, name, null);
        }

        public static ParseResult<TOutput> Fail(string name, Exception e)
        {
            return new ParseResult<TOutput>(false, default, name, e);
        }

        public TOutput GetResult()
        {
            if (!IsSuccess)
            {
                // TODO: Get a better exception type
                if (Exception != null)
                    throw new Exception($"Parse fail rule={Name ?? "unnamed"} output type={typeof(TOutput).Name}", Exception);
                throw new Exception($"Parse fail rule={Name ?? "unnamed"} output type={typeof(TOutput).Name}");
            }
            return Value;
        }
    }
}