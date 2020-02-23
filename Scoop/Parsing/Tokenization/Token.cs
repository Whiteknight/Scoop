﻿using System.Collections.Generic;
using System.Linq;
using ParserObjects;

namespace Scoop.Parsing.Tokenization
{
    public class Token : ISyntaxElement
    {
        public Token(string value, TokenType type)
        {
            Value = value;
            Type = type;
        }

        public List<string> Frontmatter { get; set; }

        public string Value { get; }
        public Location Location { get; set; }
        public TokenType Type { get; }

        public static Token EndOfInput() => new Token(null, TokenType.EndOfInput);

        public static Token Comment(string s) => new Token(s, TokenType.Comment);

        public static Token Word(string s) => new Token(s, TokenType.Word);

        public static Token String(string s) => new Token(s, TokenType.String);

        public static Token Character(string s) => new Token(s, TokenType.Character);

        public static Token Operator(string s) => new Token(s, TokenType.Operator);

        public static Token CSharpLiteral(string s) => new Token(s, TokenType.CSharpLiteral);

        public bool IsType(params TokenType[] types) => types.Any(t => Type == t);

        public bool Is(TokenType type, string value) => Type == type && Value == value;

        public bool IsOperator(params string[] operators)
        {
            if (Type != TokenType.Operator)
                return false;
            foreach (var op in operators)
            {
                if (Value == op)
                    return true;
            }

            return false;
        }

        public override string ToString()
        {
            return $"{Type}:{Value}";
        }

        public IReadOnlyList<Diagnostic> Diagnostics { get; private set; }

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
}