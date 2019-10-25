namespace Scoop.Parsers
{
    public interface IParseResult<out TOutput>
    {
        bool Success { get; }
        TOutput Value { get; }

        IParseResult<object> Untype();
    }
}