﻿using System.Collections.Generic;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    /// <summary>
    /// Takes a list of parsers and attempts each one in order. Returns as soon as the first parser
    /// succeeds
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    public class FirstParser<TOutput> : IParser<TOutput>
        where TOutput : AstNode
    {
        private readonly IReadOnlyList<IParser<AstNode>> _parsers;

        public FirstParser(params IParser<AstNode>[] parsers)
        {
            _parsers = parsers;
        }

        public TOutput TryParse(ITokenizer t)
        {
            for (int i = 0; i < _parsers.Count; i++)
            {
                var result = _parsers[i].Parse(t);
                if (result.IsSuccess)
                    return result.Value as TOutput;
            }

            return null;
        }

        public string Name { get; set; }
        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}
