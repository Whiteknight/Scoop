﻿using System;
using System.Linq;
using ParserObjects;
using ParserObjects.Parsers;
using Scoop.Parsing.Tokenization;
using Scoop.SyntaxTree;
using static ParserObjects.Parsers.ParserMethods;

namespace Scoop.Parsing.Parsers
{
    // Parser methods which depend on Token input
    // TODO: We want to move as much logic as possible out of this class to make more parsers and parser methods general-purpose
    public static class TokenParserMethods
    {
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
            var name = "Keyword:" + firstKeyword;
            if (keywords.Length > 0)
                name = name + " " + string.Join(" ", keywords);
            return Match<Token>(t => t.IsType(TokenType.Word) && (t.Value == firstKeyword || keywords.Any(k => k == t.Value)))
                .Transform(t => new KeywordNode(t))
                .Named(name);
        }

        public static IParser<Token, TOutput> Keyword<TOutput>(string firstKeyword, Func<Token, TOutput> produce)
        {
            return Match<Token>(t => t.IsType(TokenType.Word) && t.Value == firstKeyword)
                .Transform(produce)
                .Named("Keyword:" + firstKeyword);
        }

        public static IParser<Token, OperatorNode> Operator(string firstOp, params string[] operators)
        {
            var name = "Operator:" + firstOp;
            if (operators.Length > 0)
                name = name + " " + string.Join(" ", operators);
            return Match<Token>(t => t.IsType(TokenType.Operator) && (t.Value == firstOp || operators.Any(k => k == t.Value)))
                .Transform(t => new OperatorNode(t))
                .Named(name);
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
            return Match<Token>(t => t.IsType(type))
                .Transform(produce)
                .Named("Token:" + type);
        }
    }
}