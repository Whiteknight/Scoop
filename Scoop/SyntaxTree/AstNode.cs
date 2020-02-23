﻿using System.Collections.Generic;
using System.Linq;
using ParserObjects;
using Scoop.SyntaxTree.Visiting;
using Scoop.Validation;

namespace Scoop.SyntaxTree
{
    public abstract class AstNode : ISyntaxElement
    {
        public abstract AstNode Accept(IAstNodeVisitorImplementation visitor);

        // Error messages attached to this node
        public IReadOnlyList<Diagnostic> Diagnostics { get; private set; }

        // The location where this production happens in the source (may be the beginning of the production
        // or the location of the defining feature of it)
        public Location Location { get; set; }

        // Unused nodes which were generated by the parser (and which may contain diagnostics or metadata)
        // but which aren't needed in the final tree.
        public IReadOnlyList<ISyntaxElement> Unused { get; private set; }

        public void AddUnusedMembers(params ISyntaxElement[] unused)
        {
            Unused = (Unused ?? Enumerable.Empty<ISyntaxElement>()).Concat(unused).ToList();
        }

        public void AddDiagnostics(params Diagnostic[] diagnostics)
        {
            Diagnostics = (Diagnostics ?? Enumerable.Empty<Diagnostic>()).Concat(diagnostics).ToList();
        }
    }

    public static class AstNodeExtensions
    {
        /// <summary>
        /// Validate this node, extracting all diagnostics messages from this node and children
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static IReadOnlyList<Diagnostic> Validate(this AstNode node)
        {
            var errors = new List<Diagnostic>();
            new ValidationVisitor(errors).Visit(node);
            return errors;
        }
    }
}
