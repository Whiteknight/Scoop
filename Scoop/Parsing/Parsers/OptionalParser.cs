﻿using System;
using System.Collections.Generic;

namespace Scoop.Parsing.Parsers
{
    /// <summary>
    /// Attempts to parse the production and returns a default value if it does not succeed
    /// The fallback value is typically an EmptyNode but can be overridden
    /// </summary>
    public class OptionalParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly IParser<TInput, TOutput> _parser;
        private readonly Func<TOutput> _getDefault;

        public OptionalParser(IParser<TInput, TOutput> parser, Func<TOutput> getDefault = null)
        {
            _parser = parser;
            _getDefault = getDefault ?? (() => default);
        }

        public IParseResult<TOutput> Parse(ISequence<TInput> t)
        {
            var result = _parser.Parse(t);
            return result.Success ? result : Result<TOutput>.Ok(_getDefault());
        }

        IParseResult<object> IParser<TInput>.ParseUntyped(ISequence<TInput> t) => Parse(t).Untype();

        public string Name { get; set; }

        public IParser Accept(IParserVisitorImplementation visitor) => visitor.VisitOptional(this);

        public IEnumerable<IParser> GetChildren() => new[] { _parser };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_parser == find)
                return new OptionalParser<TInput, TOutput>(replace as IParser<TInput, TOutput>, _getDefault);
            return this;
        }

        public override string ToString()
        {
            if (Name == null)
                return "Optional." + _parser;
            return $"Optional={Name}.{_parser}";
        }
    }
}