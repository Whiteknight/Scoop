using Scoop.Parsing.Tokenization;
using Scoop.SyntaxTree.Visiting;

namespace Scoop.SyntaxTree
{
    public class CharNode : AstNode
    {
        public CharNode(char value, Location location = null)
        {
            Value = $"'{value}'";
            Location = location;
        }

        public CharNode(Token t)
        {
            Value = t.Value;
            Location = t.Location;
            if (!t.Diagnostics.IsNullOrEmpty())
                AddUnusedMembers(t);
        }

        public string Value { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitChar(this);
    }
}