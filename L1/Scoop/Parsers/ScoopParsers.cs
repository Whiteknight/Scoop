using System;
using System.Collections.Generic;
using System.Linq;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    public static class ScoopParsers
    {
        public static IParser<AstNode> ApplyPostfix(IParser<AstNode> left, Func<IParser<AstNode>, IParser<AstNode>> produce)
        {
            return new ApplyPostfixParser(left, produce);
        }

        /// <summary>
        /// Get a reference to a parser. Avoids circular dependencies in the grammar
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="getParser"></param>
        /// <returns></returns>
        public static IParser<TOutput> Deferred<TOutput>(Func<IParser<TOutput>> getParser)
        {
            return new DeferredParser<TOutput>(getParser);
        }

        /// <summary>
        /// Return a node which represents an error in the parse
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="consumeOne"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static IParser<TOutput> Error<TOutput>(bool consumeOne, string errorMessage)
            where TOutput : AstNode, new()
        {
            return new ErrorParser<TOutput>(consumeOne, errorMessage);
        }

        /// <summary>
        /// Return the reuslt of the first parser which succeeds
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parsers"></param>
        /// <returns></returns>
        public static IParser<TOutput> First<TOutput>(params IParser<TOutput>[] parsers)
            where TOutput : AstNode
        {
            return new FirstParser<TOutput>(parsers.Cast<IParser<AstNode>>().ToArray());
        }

        /// <summary>
        /// Parse a left-associative single operator precedence level. Parse an item as the left-hand-side,
        /// then try to parse an operator and a right-hand-side. If possible, reduce to a single node, set
        /// that as the new left-hand-side, and continue
        /// </summary>
        /// <param name="left"></param>
        /// <param name="operatorParser"></param>
        /// <param name="right"></param>
        /// <param name="producer"></param>
        /// <returns></returns>
        public static IParser<AstNode> Infix(IParser<AstNode> left, IParser<OperatorNode> operatorParser, IParser<AstNode> right, Func<AstNode, OperatorNode, AstNode, AstNode> producer)
        {
            return new InfixOperatorParser(left, operatorParser, right, producer);
        }

        /// <summary>
        /// Parse a list of zero or more items.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="produce"></param>
        /// <returns></returns>
        public static IParser<ListNode<TOutput>> List<TItem, TOutput>(IParser<TItem> p, Func<IReadOnlyList<TItem>, ListNode<TOutput>> produce)
            where TOutput : AstNode
        {
            return new ListParser<TOutput, TItem>(p, produce);
        }

        /// <summary>
        /// Attempt to parse an item and return a default value otherwise
        /// </summary>
        /// <param name="p"></param>
        /// <param name="getDefault"></param>
        /// <returns></returns>
        public static IParser<AstNode> Optional(IParser<AstNode> p, Func<AstNode> getDefault = null)
        {
            return new OptionalParser(p, getDefault);
        }

        /// <summary>
        /// Produce an empty or default node without consuming anything out of the tokenizer
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="produce"></param>
        /// <returns></returns>
        public static IParser<TOutput> Produce<TOutput>(Func<TOutput> produce)
        {
            return new ProduceParser<TOutput>(produce);
        }

        /// <summary>
        /// Parse a required item. If the parse fails, return a default version with diagnostic
        /// error information for the missing tokens
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static IParser<TOutput> Required<TOutput>(IParser<TOutput> p, string errorMessage)
            where TOutput : AstNode, new()
        {
            return new RequiredParser<TOutput>(p, l => new TOutput().WithDiagnostics(l, errorMessage));
        }

        /// <summary>
        /// Parse a required item. If the parse fails, produce a default version and attach a
        /// diagnostic error message
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="produce"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static IParser<TOutput> Required<TOutput>(IParser<TOutput> p, Func<TOutput> produce, string errorMessage)
            where TOutput : AstNode
        {
            return new RequiredParser<TOutput>(p, l => produce().WithDiagnostics(l, errorMessage));
        }

        /// <summary>
        /// Parse a list of items separated by some separator production
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p"></param>
        /// <param name="separator"></param>
        /// <param name="produce"></param>
        /// <param name="atLeastOne"></param>
        /// <returns></returns>
        public static IParser<ListNode<TOutput>> SeparatedList<TItem, TOutput>(IParser<TItem> p, IParser<AstNode> separator, Func<IReadOnlyList<TItem>, ListNode<TOutput>> produce, bool atLeastOne = false)
            where TOutput : AstNode
            where TItem : AstNode
        {
            return new SeparatedListParser<TOutput, TItem>(p, separator, produce, atLeastOne);
        }

        /// <summary>
        /// Parse a sequence of productions and reduce them into a single output. If any item fails, rollback all and
        /// return failure
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="produce"></param>
        /// <returns></returns>
        public static IParser<TOutput> Sequence<T1, T2, TOutput>(IParser<T1> p1, IParser<T2> p2, Func<T1, T2, TOutput> produce)
            where T1: AstNode
            where T2: AstNode
        {
            return new SequenceParser<TOutput>(
                new IParser<AstNode>[] { p1, p2 },
                (list) => produce((T1)list[0], (T2)list[1]));
        }

        public static IParser<TOutput> Sequence<T1, T2, T3, TOutput>(IParser<T1> p1, IParser<T2> p2, IParser<T3> p3, Func<T1, T2, T3, TOutput> produce)
            where T1 : AstNode
            where T2 : AstNode
            where T3 : AstNode
        {
            return new SequenceParser<TOutput>(
                new IParser<AstNode>[] { p1, p2, p3 },
                (list) => produce((T1)list[0], (T2)list[1], (T3)list[2]));
        }

        public static IParser<TOutput> Sequence<T1, T2, T3, T4, TOutput>(IParser<T1> p1, IParser<T2> p2, IParser<T3> p3, IParser<T4> p4, Func<T1, T2, T3, T4, TOutput> produce)
            where T1 : AstNode
            where T2 : AstNode
            where T3 : AstNode
            where T4 : AstNode
        {
            return new SequenceParser<TOutput>(
                new IParser<AstNode>[] { p1, p2, p3, p4 },
                (list) => produce((T1)list[0], (T2)list[1], (T3)list[2], (T4)list[3]));
        }

        public static IParser<TOutput> Sequence<T1, T2, T3, T4, T5, TOutput>(IParser<T1> p1, IParser<T2> p2, IParser<T3> p3, IParser<T4> p4, IParser<T5> p5, Func<T1, T2, T3, T4, T5, TOutput> produce)
            where T1 : AstNode
            where T2 : AstNode
            where T3 : AstNode
            where T4 : AstNode
            where T5 : AstNode
        {
            return new SequenceParser<TOutput>(
                new IParser<AstNode>[] { p1, p2, p3, p4, p5 },
                (list) => produce((T1)list[0], (T2)list[1], (T3)list[2], (T4)list[3], (T5)list[4]));
        }

        public static IParser<TOutput> Sequence<T1, T2, T3, T4, T5, T6, TOutput>(IParser<T1> p1, IParser<T2> p2, IParser<T3> p3, IParser<T4> p4, IParser<T5> p5, IParser<T6> p6, Func<T1, T2, T3, T4, T5, T6, TOutput> produce)
            where T1 : AstNode
            where T2 : AstNode
            where T3 : AstNode
            where T4 : AstNode
            where T5 : AstNode
            where T6 : AstNode
        {
            return new SequenceParser<TOutput>(
                new IParser<AstNode>[] { p1, p2, p3, p4, p5, p6 },
                (list) => produce((T1)list[0], (T2)list[1], (T3)list[2], (T4)list[3], (T5)list[4], (T6)list[5]));
        }

        public static IParser<TOutput> Sequence<T1, T2, T3, T4, T5, T6, T7, TOutput>(IParser<T1> p1, IParser<T2> p2, IParser<T3> p3, IParser<T4> p4, IParser<T5> p5, IParser<T6> p6, IParser<T7> p7, Func<T1, T2, T3, T4, T5, T6, T7, TOutput> produce)
            where T1 : AstNode
            where T2 : AstNode
            where T3 : AstNode
            where T4 : AstNode
            where T5 : AstNode
            where T6 : AstNode
            where T7 : AstNode
        {
            return new SequenceParser<TOutput>(
                new IParser<AstNode>[] { p1, p2, p3, p4, p5, p6, p7 },
                (list) => produce((T1)list[0], (T2)list[1], (T3)list[2], (T4)list[3], (T5)list[4], (T6)list[5], (T7)list[6]));
        }

        public static IParser<TOutput> Sequence<T1, T2, T3, T4, T5, T6, T7, T8, TOutput>(IParser<T1> p1, IParser<T2> p2, IParser<T3> p3, IParser<T4> p4, IParser<T5> p5, IParser<T6> p6, IParser<T7> p7, IParser<T8> p8, Func<T1, T2, T3, T4, T5, T6, T7, T8, TOutput> produce)
            where T1 : AstNode
            where T2 : AstNode
            where T3 : AstNode
            where T4 : AstNode
            where T5 : AstNode
            where T6 : AstNode
            where T7 : AstNode
            where T8 : AstNode
        {
            return new SequenceParser<TOutput>(
                new IParser<AstNode>[] { p1, p2, p3, p4, p5, p6, p7, p8 },
                (list) => produce((T1)list[0], (T2)list[1], (T3)list[2], (T4)list[3], (T5)list[4], (T6)list[5], (T7)list[6], (T8)list[7]));
        }

        public static IParser<TOutput> Sequence<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOutput>(IParser<T1> p1, IParser<T2> p2, IParser<T3> p3, IParser<T4> p4, IParser<T5> p5, IParser<T6> p6, IParser<T7> p7, IParser<T8> p8, IParser<T9> p9, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOutput> produce)
            where T1 : AstNode
            where T2 : AstNode
            where T3 : AstNode
            where T4 : AstNode
            where T5 : AstNode
            where T6 : AstNode
            where T7 : AstNode
            where T8 : AstNode
            where T9 : AstNode
        {
            return new SequenceParser<TOutput>(
                new IParser<AstNode>[] { p1, p2, p3, p4, p5, p6, p7, p8, p9 },
                (list) => produce((T1)list[0], (T2)list[1], (T3)list[2], (T4)list[3], (T5)list[4], (T6)list[5], (T7)list[6], (T8)list[7], (T9)list[8]));
        }

        /// <summary>
        /// Parse a single token of an expected type and return the appropriate output node
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="type"></param>
        /// <param name="produce"></param>
        /// <returns></returns>
        public static IParser<TOutput> Token<TOutput>(TokenType type, Func<Token, TOutput> produce)
            where TOutput : AstNode
        {
            return new TokenParser<TOutput>(type, produce);
        }

        /// <summary>
        /// Transform one node into another node to fit into the grammar
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parser"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static IParser<TOutput> Transform<TInput, TOutput>(IParser<TInput> parser, Func<TInput, TOutput> transform)
            where TOutput : AstNode
            where TInput : AstNode
        {
            return new TransformParser<TOutput, TInput>(parser, transform);
        }
    }
}
