﻿using System.Collections.Generic;
using System.Linq;
using ParserObjects;

namespace Scoop
{
    public interface ISyntaxElement
    {
        Location Location { get; }
        IReadOnlyList<ISyntaxElement> Unused { get; }
        IReadOnlyList<Diagnostic> Diagnostics { get; }
        void AddUnusedMembers(params ISyntaxElement[] unused);
        void AddDiagnostics(params Diagnostic[] diagnostics);
    }

    public static class SyntaxElementExtensions
    {
        /// <summary>
        /// Add unused nodes which are generated by the parser but which are not required for the final tree
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <param name="element"></param>
        /// <param name="unused"></param>
        /// <returns></returns>
        public static TElement WithUnused<TElement>(this TElement element, params ISyntaxElement[] unused)
            where TElement : ISyntaxElement
        {
            element.AddUnusedMembers(unused);
            return element;
        }

        /// <summary>
        /// Add diagnostic/error messages to this node
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <param name="element"></param>
        /// <param name="diagnostics"></param>
        /// <returns></returns>
        public static TElement WithDiagnostics<TElement>(this TElement element, params Diagnostic[] diagnostics)
            where TElement : ISyntaxElement
        {
            element.AddDiagnostics(diagnostics);
            return element;
        }

        public static TElement WithDiagnostics<TElement>(this TElement element, ICollection<Diagnostic> diagnostics)
            where TElement : ISyntaxElement
        {
            if (!diagnostics.IsNullOrEmpty())
                element.AddDiagnostics(diagnostics.ToArray());
            return element;
        }

        /// <summary>
        /// Add diagnostic/error messages to this node
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <param name="element"></param>
        /// <param name="l"></param>
        /// <param name="diagnostics"></param>
        /// <returns></returns>
        public static TElement WithDiagnostics<TElement>(this TElement element, Location l, params string[] diagnostics)
            where TElement : ISyntaxElement
        {
            if (diagnostics.IsNullOrEmpty())
                return element;
            element.AddDiagnostics(diagnostics.Select(d => new Diagnostic(d, l)).ToArray());
            return element;
        }

        public static TElement WithDiagnostics<TElement>(this TElement element, params string[] diagnostics)
            where TElement : ISyntaxElement
        {
            element.AddDiagnostics(diagnostics.Select(d => new Diagnostic(d, null)).ToArray());
            return element;
        }
    }
}