using Scoop.SyntaxTree;

namespace Scoop.Tests.Utility
{
    public static class AstNodeExtensions
    {
        public static AstNodeAssertions Should(this AstNode node)
        {
            return new AstNodeAssertions(node);
        }
    }
}
