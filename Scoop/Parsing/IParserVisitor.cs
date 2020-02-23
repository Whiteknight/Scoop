using ParserObjects;

namespace Scoop.Parsing
{
    public interface IParserVisitor
    {
        IParser Visit(IParser parser);
    }
}
