using Scoop.SyntaxTree;
using Scoop.SyntaxTree.Visiting;

namespace Scoop.Transpiling
{
    public partial class CSharpTranspileVisitor : IAstNodeVisitor
    {
        public AstNode Visit(AstNode node) => node?.Accept(this);
    }
}
