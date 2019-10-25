using System.Collections.Generic;
using Scoop.SyntaxTree;
using Scoop.SyntaxTree.Visiting;

namespace Scoop.Validation
{
    public class ValidationVisitor : AstNodeVisitorBase
    {
        private readonly ICollection<Diagnostic> _diagnostics;

        public ValidationVisitor(ICollection<Diagnostic> diagnostics)
        {
            _diagnostics = diagnostics;
        }

        public override AstNode Visit(AstNode node)
        {
            if (node == null)
                return null;
            AddDiagnostics(node);
            if (!node.Unused.IsNullOrEmpty())
            {
                foreach (var un in node.Unused)
                    AddDiagnostics(un);
            }
            return node.Accept(this);
        }

        private void AddDiagnostics(ISyntaxElement node)
        {
            if (node.Diagnostics.IsNullOrEmpty())
                return;
            foreach (var d in node.Diagnostics)
                _diagnostics.Add(d.AddLocation(node.Location));
        }
    }
}
