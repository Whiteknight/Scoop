﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Scoop.Parsing.Parsers
{
    public class MatchSequenceParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly TInput[] _find;
        private readonly Func<TInput[], TOutput> _produce;

        public MatchSequenceParser(IEnumerable<TInput> find, Func<TInput[], TOutput> produce)
        {
            _find = find.ToArray();
            _produce = produce;
        }

        public IParseResult<TOutput> Parse(ISequence<TInput> t)
        {
            // small optimization, don't allocate a buffer if we only need one item
            if (_find.Length == 1)
                return t.Peek().Equals(_find[0]) ? Result<TOutput>.Ok(_produce(new[] { t.GetNext() })) : Result<TOutput>.Fail();

            var buffer = new TInput[_find.Length];
            for (var i = 0; i < _find.Length; i++)
            {
                var c = t.GetNext();
                buffer[i] = c;
                if (c.Equals(_find[i]))
                    continue;

                for (; i >= 0; i--)
                    t.PutBack(buffer[i]);
                return Result<TOutput>.Fail();
            }

            return Result<TOutput>.Ok(_produce(buffer));
        }

        public IParseResult<object> ParseUntyped(ISequence<TInput> t) => (IParseResult<object>)Parse(t);

        public string Name { get; set; }

        public IParser Accept(IParserVisitorImplementation visitor) => this;

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;
    }
}