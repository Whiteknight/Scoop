using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public interface IHasAttributes
    {
        ListNode<AttributeNode> Attributes { get; set; }
    }
}
