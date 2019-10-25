namespace Scoop.Parsing
{
    public interface IParseResult<out TOutput>
    {
        bool Success { get; }
        TOutput Value { get; }

        IParseResult<object> Untype();
    }
}