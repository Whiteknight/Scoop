﻿using System;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    /// <summary>
    /// Attempts to parse the production and returns a default value if it does not succeed
    /// The fallback value is typically an EmptyNode but can be overridden
    /// </summary>
    public class OptionalParser : IParser<AstNode>
    {
        private readonly IParser<AstNode> _parser;
        private readonly Func<AstNode> _getDefault;

        public OptionalParser(IParser<AstNode> parser, Func<AstNode> getDefault = null)
        {
            _parser = parser;
            _getDefault = getDefault ?? (() => new EmptyNode());
        }

        public AstNode TryParse(ITokenizer t)
        {
            try
            {
                var result = _parser.Parse(t);
                return result.IsSuccess ? result.GetResult() : _getDefault();
            }
            catch
            {
                return _getDefault();
            }
        }

        public string Name { get; set; }
        public override string ToString()
        {
            if (Name == null)
                return "Optional." + _parser.ToString();
            return $"Optional={Name}.{_parser}";
        }
    }
}