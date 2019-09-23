﻿using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    /// <summary>
    /// Represents an error situation. Returns a default node with error information
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    public class ErrorParser<TOutput> : IParser<TOutput>
        where TOutput : AstNode, new()
    {
        private readonly bool _consumeOne;
        private readonly string _errorMessage;

        public ErrorParser(bool consumeOne, string errorMessage)
        {
            _consumeOne = consumeOne;
            _errorMessage = errorMessage;
        }

        public TOutput TryParse(ITokenizer t)
        {
            if (_consumeOne)
                t.Advance();
            return new TOutput().WithDiagnostics(t.Peek().Location, _errorMessage);
        }

        public string Name { get; set; }
    }
}
