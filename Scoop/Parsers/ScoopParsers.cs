using System;
using System.Collections.Generic;
using System.Linq;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    public static class ScoopParsers
    {
        public static IParser<Token, AstNode> ApplyPostfix(IParser<Token, AstNode> left, Func<IParser<Token, AstNode>, IParser<Token, AstNode>> getRight)
        {
            return new ApplyPostfixParser(left, getRight);
        }

        /// <summary>
        /// Get a reference to a parser. Avoids circular dependencies in the grammar
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="getParser"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Deferred<TInput, TOutput>(Func<IParser<TInput, TOutput>> getParser)
            where TOutput : AstNode
        {
            return new DeferredParser<TInput, TOutput>(getParser);
        }

        /// <summary>
        /// Return a node which represents an error in the parse. Returns a synthetic node
        /// with diagnostic information about the underlying syntax being missing
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="consumeOne"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static IParser<Token, TOutput> Error<TOutput>(bool consumeOne, string errorMessage)
            where TOutput : AstNode, new()
        {
            return new ErrorParser<TOutput>(consumeOne, errorMessage);
        }

        /// <summary>
        /// Always returns null/failure. Useful to create a placeholder in the tree where
        /// we may want to make a replacement later
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TInput"></typeparam>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Fail<TInput, TOutput>()
            where TOutput : AstNode
        {
            return new FailParser<TInput, TOutput>();
        }

        /// <summary>
        /// Return the reuslt of the first parser which succeeds
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="parsers"></param>
        /// <returns></returns>
        //public static IParser<TInput, TOutput> First<TInput, TOutput>(params IParser<TInput>[] parsers)
        //{
        //    // This variant allows us to put off output type-checking until runtime, but there's a possibility
        //    // that the parser returns a type of output which isn't compatible with TOutput and we will get
        //    // a runtime exception.
        //    return new FirstParser<TInput, TOutput>(parsers);
        //}

        public static IParser<TInput, TOutput> First<TInput, TOutput>(params IParser<TInput, TOutput>[] parsers)
        {
            return new FirstParser<TInput, TOutput>(parsers);
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
        public static IParser<Token, AstNode> Infix(IParser<Token, AstNode> left, IParser<Token, OperatorNode> operatorParser, IParser<Token, AstNode> right, Func<AstNode, OperatorNode, AstNode, AstNode> producer)
        {
            return new InfixOperatorParser(left, operatorParser, right, producer);
        }

        public static IParser<Token, KeywordNode> Keyword(string firstKeyword, params string[] keywords)
        {
            return new PredicateParser<Token, KeywordNode>(t => t.IsType(TokenType.Word) && (t.Value == firstKeyword || keywords.Any(k => k == t.Value)), t => new KeywordNode(t));
        }

        public static IParser<Token, TOutput> Keyword<TOutput>(string firstKeyword, Func<Token, TOutput> produce)
        {
            return new PredicateParser<Token, TOutput>(t => t.IsType(TokenType.Word) && t.Value == firstKeyword, produce);
        }

        /// <summary>
        /// Parse a list of zero or more items.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="p"></param>
        /// <param name="produce"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> List<TInput, TItem, TOutput>(IParser<TInput, TItem> p, Func<IReadOnlyList<TItem>, TOutput> produce)
        {
            return new ListParser<TInput, TItem, TOutput>(p, produce);
        }

        public static IParser<Token, OperatorNode> Operator(string firstOp, params string[] operators)
        {
            return new PredicateParser<Token, OperatorNode>(t => t.IsType(TokenType.Operator) && (t.Value == firstOp || operators.Any(k => k == t.Value)), t => new OperatorNode(t));
        }

        /// <summary>
        /// Attempt to parse an item and return a default value otherwise
        /// </summary>
        /// <param name="p"></param>
        /// <param name="getDefault"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Optional<TInput, TOutput>(IParser<TInput, TOutput> p, Func<TOutput> getDefault = null)
            where TOutput : AstNode
        {
            return new OptionalParser<TInput, TOutput>(p, getDefault);
        }

        /// <summary>
        /// Produce an empty or default node without consuming anything out of the tokenizer
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="produce"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Produce<TInput, TOutput>(Func<TOutput> produce)
        {
            return new ProduceParser<TInput, TOutput>(produce);
        }

        public static IParser<TInput, TOutput> Replaceable<TInput, TOutput>(IParser<TInput, TOutput> defaultValue)
        {
            return new ReplaceableParser<TInput, TOutput>(defaultValue);
        }

        /// <summary>
        /// Parse a required item. If the parse fails, return a default version with diagnostic
        /// error information for the missing tokens
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="p"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Required<TInput, TOutput>(IParser<TInput, TOutput> p, string errorMessage)
            where TOutput : AstNode, new()
        {
            return new RequiredParser<TInput, TOutput>(p, l => new TOutput().WithDiagnostics(l, errorMessage));
        }

        /// <summary>
        /// Parse a required item. If the parse fails, produce a default version and attach a
        /// diagnostic error message
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="p"></param>
        /// <param name="produce"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Required<TInput, TOutput>(IParser<TInput, TOutput> p, Func<TOutput> produce, string errorMessage)
            where TOutput : AstNode
        {
            return new RequiredParser<TInput, TOutput>(p, l => produce().WithDiagnostics(l, errorMessage));
        }

        /// <summary>
        /// Parse a list of items separated by some separator production
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TSeparator"></typeparam>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="p"></param>
        /// <param name="separator"></param>
        /// <param name="produce"></param>
        /// <param name="getMissingItem"></param>
        /// <param name="atLeastOne"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> SeparatedList<TInput, TItem, TSeparator, TOutput>(IParser<TInput, TItem> p, IParser<TInput, TSeparator> separator, Func<IReadOnlyList<TItem>, TOutput> produce, bool atLeastOne = false)
            where TOutput : AstNode
        {
            return new SeparatedListParser<TInput, TItem, TSeparator, TOutput>(p, separator, produce, atLeastOne);
        }

        /// <summary>
        /// Parse a sequence of productions and reduce them into a single output. If any item fails, rollback all and
        /// return failure
        /// </summary>
        /// <typeparam name="TInput, T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="produce"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Sequence<TInput, T1, T2, TOutput>(IParser<TInput, T1> p1, IParser<TInput, T2> p2, Func<T1, T2, TOutput> produce)
        {
            return new SequenceParser<TInput, TOutput>(
                new IParser<TInput>[] { p1, p2 },
                (list) => produce((T1)list[0], (T2)list[1]));
        }

        public static IParser<TInput, TOutput> Sequence<TInput, T1, T2, T3, TOutput>(IParser<TInput, T1> p1, IParser<TInput, T2> p2, IParser<TInput, T3> p3, Func<T1, T2, T3, TOutput> produce)
        {
            return new SequenceParser<TInput, TOutput>(
                new IParser<TInput>[] { p1, p2, p3 },
                (list) => produce((T1)list[0], (T2)list[1], (T3)list[2]));
        }

        public static IParser<TInput, TOutput> Sequence<TInput, T1, T2, T3, T4, TOutput>(IParser<TInput, T1> p1, IParser<TInput, T2> p2, IParser<TInput, T3> p3, IParser<TInput, T4> p4, Func<T1, T2, T3, T4, TOutput> produce)
        {
            return new SequenceParser<TInput, TOutput>(
                new IParser<TInput>[] { p1, p2, p3, p4 },
                (list) => produce((T1)list[0], (T2)list[1], (T3)list[2], (T4)list[3]));
        }

        public static IParser<TInput, TOutput> Sequence<TInput, T1, T2, T3, T4, T5, TOutput>(IParser<TInput, T1> p1, IParser<TInput, T2> p2, IParser<TInput, T3> p3, IParser<TInput, T4> p4, IParser<TInput, T5> p5, Func<T1, T2, T3, T4, T5, TOutput> produce)
        {
            return new SequenceParser<TInput, TOutput>(
                new IParser<TInput>[] { p1, p2, p3, p4, p5 },
                (list) => produce((T1)list[0], (T2)list[1], (T3)list[2], (T4)list[3], (T5)list[4]));
        }

        public static IParser<TInput, TOutput> Sequence<TInput, T1, T2, T3, T4, T5, T6, TOutput>(IParser<TInput, T1> p1, IParser<TInput, T2> p2, IParser<TInput, T3> p3, IParser<TInput, T4> p4, IParser<TInput, T5> p5, IParser<TInput, T6> p6, Func<T1, T2, T3, T4, T5, T6, TOutput> produce)
        {
            return new SequenceParser<TInput, TOutput>(
                new IParser<TInput>[] { p1, p2, p3, p4, p5, p6 },
                (list) => produce((T1)list[0], (T2)list[1], (T3)list[2], (T4)list[3], (T5)list[4], (T6)list[5]));
        }

        public static IParser<TInput, TOutput> Sequence<TInput, T1, T2, T3, T4, T5, T6, T7, TOutput>(IParser<TInput, T1> p1, IParser<TInput, T2> p2, IParser<TInput, T3> p3, IParser<TInput, T4> p4, IParser<TInput, T5> p5, IParser<TInput, T6> p6, IParser<TInput, T7> p7, Func<T1, T2, T3, T4, T5, T6, T7, TOutput> produce)
        {
            return new SequenceParser<TInput, TOutput>(
                new IParser<TInput>[] { p1, p2, p3, p4, p5, p6, p7 },
                (list) => produce((T1)list[0], (T2)list[1], (T3)list[2], (T4)list[3], (T5)list[4], (T6)list[5], (T7)list[6]));
        }

        public static IParser<TInput, TOutput> Sequence<TInput, T1, T2, T3, T4, T5, T6, T7, T8, TOutput>(IParser<TInput, T1> p1, IParser<TInput, T2> p2, IParser<TInput, T3> p3, IParser<TInput, T4> p4, IParser<TInput, T5> p5, IParser<TInput, T6> p6, IParser<TInput, T7> p7, IParser<TInput, T8> p8, Func<T1, T2, T3, T4, T5, T6, T7, T8, TOutput> produce)
        {
            return new SequenceParser<TInput, TOutput>(
                new IParser<TInput>[] { p1, p2, p3, p4, p5, p6, p7, p8 },
                (list) => produce((T1)list[0], (T2)list[1], (T3)list[2], (T4)list[3], (T5)list[4], (T6)list[5], (T7)list[6], (T8)list[7]));
        }

        public static IParser<TInput, TOutput> Sequence<TInput, T1, T2, T3, T4, T5, T6, T7, T8, T9, TOutput>(IParser<TInput, T1> p1, IParser<TInput, T2> p2, IParser<TInput, T3> p3, IParser<TInput, T4> p4, IParser<TInput, T5> p5, IParser<TInput, T6> p6, IParser<TInput, T7> p7, IParser<TInput, T8> p8, IParser<TInput, T9> p9, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOutput> produce)
        {
            return new SequenceParser<TInput, TOutput>(
                new IParser<TInput>[] { p1, p2, p3, p4, p5, p6, p7, p8, p9 },
                (list) => produce((T1)list[0], (T2)list[1], (T3)list[2], (T4)list[3], (T5)list[4], (T6)list[5], (T7)list[6], (T8)list[7], (T9)list[8]));
        }

        /// <summary>
        /// Parse a single token of an expected type and return the appropriate output node
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="type"></param>
        /// <param name="produce"></param>
        /// <returns></returns>
        public static IParser<Token, TOutput> Token<TOutput>(TokenType type, Func<Token, TOutput> produce)
        {
            return new PredicateParser<Token, TOutput>(t => t.IsType(type), produce);
        }

        /// <summary>
        /// Transform one node into another node to fit into the grammar
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TMiddle"></typeparam>
        /// <param name="parser"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Transform<TInput, TMiddle, TOutput>(IParser<TInput, TMiddle> parser, Func<TMiddle, TOutput> transform)
        {
            return new TransformParser<TInput, TMiddle, TOutput>(parser, transform);
        }
    }
}
