using System;
using System.Collections.Generic;
using System.Linq;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    public static class ScoopParsers
    {
        public static IParser<TOutput> Deferred<TOutput>(Func<IParser<TOutput>> getParser)
        {
            return new DeferredParser<TOutput>(getParser);
        }

        public static IParser<TOutput> First<TOutput>(params IParser<TOutput>[] parsers)
            where TOutput : AstNode
        {
            return new FirstParser<TOutput>(parsers.Cast<IParser<AstNode>>().ToArray());
        }

        public static IParser<TOutput> Required<TOutput>(IParser<TOutput> p, Func<Location, TOutput> otherwise)
            where TOutput : AstNode
        {
            return new RequiredParser<TOutput>(p, otherwise);
        }

        public static IParser<OperatorNode> RequireOperator(string op)
        {
            return new RequiredParser<OperatorNode>(new OperatorParser(op), l => new OperatorNode(op).WithDiagnostics(l, $"Missing {op}"));
        }

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

        public static IParser<ListNode<TOutput>> List<TItem, TOutput>(IParser<TItem> p, Func<IReadOnlyList<TItem>, ListNode<TOutput>> produce)
            where TOutput : AstNode
        {
            return new ListParser<TOutput, TItem>(p, produce);
        }

        public static IParser<ListNode<TOutput>> SeparatedList<TItem, TOutput>(IParser<TItem> p, IParser<AstNode> separator, Func<IReadOnlyList<TItem>, ListNode<TOutput>> produce)
            where TOutput : AstNode
            where TItem : AstNode
        {
            return new SeparatedListParser<TOutput, TItem>(p, separator, produce);
        }

        public static IParser<AstNode> Optional(IParser<AstNode> p, Func<AstNode> getDefault = null)
        {
            return new OptionalParser(p, getDefault);
        }

        public static IParser<TOutput> Transform<TInput, TOutput>(IParser<TInput> parser, Func<TInput, TOutput> transform)
            where TOutput : AstNode
            where TInput : AstNode
        {
            return new TransformParser<TOutput, TInput>(parser, transform);
        }

        public static IParser<TOutput> Infix<TOutput, TOperator>(IParser<TOutput> itemParser, IParser<TOperator> operatorParser, Func<TOutput, TOperator, TOutput, TOutput> producer)
            where TOutput : AstNode
            where TOperator : AstNode
        {
            return new InfixOperatorParser<TOutput, TOperator>(itemParser, operatorParser, producer);
        }

        public static IParser<TOutput> Token<TOutput>(TokenType type, Func<Token, TOutput> produce)
            where TOutput : AstNode
        {
            return new TokenParser<TOutput>(type, produce);
        }
    }
}
