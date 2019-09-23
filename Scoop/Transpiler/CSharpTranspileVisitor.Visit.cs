using Scoop.SyntaxTree;

namespace Scoop.Transpiler
{
    public partial class CSharpTranspileVisitor : IAstNodeVisitor
    {
        public AstNode Visit(AstNode node) => node?.Accept(this);
    }
}
