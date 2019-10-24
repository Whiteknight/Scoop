using System;
using System.Linq;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    // Parser methods which depend on Token input
    // TODO: We want to move as much logic as possible out of this class to make more parsers and parser methods general-purpose
    public static class TokenParserMethods
    {
        public static IParser<Token, AstNode> ApplyPostfix(IParser<Token, AstNode> left, Func<IParser<Token, AstNode>, IParser<Token, AstNode>> getRight)
        {
            return new ApplyPostfixParser<Token, AstNode>(left, getRight);
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

        public static IParser<Token, OperatorNode> Operator(string firstOp, params string[] operators)
        {
            return new PredicateParser<Token, OperatorNode>(t => t.IsType(TokenType.Operator) && (t.Value == firstOp || operators.Any(k => k == t.Value)), t => new OperatorNode(t));
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
    }
}