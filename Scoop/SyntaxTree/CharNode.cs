using Scoop.SyntaxTree.Visiting;
using Scoop.Tokenization;

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
        }

        public string Value { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitChar(this);
    }
}