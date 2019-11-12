﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Scoop.Parsing.Parsers
{
    /// <summary>
    /// Parser to produce an output node unconditionally. Consumes no input.
    /// This is used to provide a default node value
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TInput"></typeparam>
    public class ProduceParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly Func<ISequence<TInput>, TOutput> _produce;

        public ProduceParser(Func<ISequence<TInput>, TOutput> produce)
        {
            _produce = produce;
        }

        public IParseResult<TOutput> Parse(ISequence<TInput> t) => Result<TOutput>.Ok(_produce(t));

        IParseResult<object> IParser<TInput>.ParseUntyped(ISequence<TInput> t) => Parse(t).Untype();

        public string Name { get; set; }

        public IParser Accept(IParserVisitorImplementation visitor) => visitor.VisitProduce(this);

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}