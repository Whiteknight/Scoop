using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public interface IHasAttributes
    {
        List<AttributeNode> Attributes { get; set; }
    }
}
