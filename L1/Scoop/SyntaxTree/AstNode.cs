using System.Collections.Generic;
using System.Linq;
using Scoop.Validation;

namespace Scoop.SyntaxTree
{
    public class Diagnostic
    {
        public string ErrorMessage { get; }
        public Location Location { get; }

        public Diagnostic(string errorMessage, Location location)
        {
            ErrorMessage = errorMessage;
            Location = location;
        }
    }

    public abstract class AstNode
    {
        protected AstNode(IReadOnlyList<Diagnostic> diagnostics)
        {
            Diagnostics = diagnostics;
        }

        public abstract AstNode Accept(IAstNodeVisitorImplementation visitor);

        //public string ToString()
        //{
        //    var sb = new StringBuilder();
        //    var visitor = new SqlServerStringifyVisitor(sb);
        //    visitor.Visit(this);
        //    return sb.ToString();
        //}

        public IReadOnlyList<Diagnostic> Diagnostics { get; private set; }
        public Location Location { get; set; }
        public IReadOnlyList<AstNode> Unused { get; private set; }

        public void AddUnusedMembers(params AstNode[] unused)
        {
            Unused = unused;
        }

        public void AddDiagnostics(params Diagnostic[] diagnostics)
        {
            Diagnostics = diagnostics;
        }
    }

    public static class AstNodeExtensions
    {
        public static TNode WithUnused<TNode>(this TNode node, params AstNode[] unused)
            where TNode : AstNode
        {
            node.AddUnusedMembers(unused);
            return node;
        }

        public static TNode WithDiagnostics<TNode>(this TNode node, params Diagnostic[] diagnostics)
            where TNode : AstNode
        {
            node.AddDiagnostics(diagnostics);
            return node;
        }

        public static TNode WithDiagnostics<TNode>(this TNode node, Location l, params string[] diagnostics)
            where TNode : AstNode
        {
            node.AddDiagnostics(diagnostics.Select(d => new Diagnostic(d, l)).ToArray());
            return node;
        }

        public static IReadOnlyList<Diagnostic> Validate(this AstNode node)
        {
            var errors = new List<Diagnostic>();
            new ValidationVisitor(errors).Visit(node);
            return errors;
        }
    }
}
