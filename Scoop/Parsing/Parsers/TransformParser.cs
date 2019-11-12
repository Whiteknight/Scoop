﻿using System;
using System.Collections.Generic;

namespace Scoop.Parsing.Parsers
{
    /// <summary>
    /// Transforms the output of one parser into a different form based on context
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TMiddle"></typeparam>
    public class TransformParser<TInput, TMiddle, TOutput> : IParser<TInput, TOutput>
    {
        private readonly IParser<TInput, TMiddle> _parser;
        private readonly Func<TMiddle, TOutput> _transform;

        public TransformParser(IParser<TInput, TMiddle> parser, Func<TMiddle, TOutput> transform)
        {
            _parser = parser;
            _transform = transform;
        }

        public IParseResult<TOutput> Parse(ISequence<TInput> t)
        {
            var result = _parser.Parse(t);
            if (!result.Success)
                return Result<TOutput>.Fail();
            return Result<TOutput>.Ok(_transform(result.Value));
        }

        IParseResult<object> IParser<TInput>.ParseUntyped(ISequence<TInput> t) => Parse(t).Untype();

        public string Name { get; set; }

        public IParser Accept(IParserVisitorImplementation visitor) => visitor.VisitTransform(this);

        public IEnumerable<IParser> GetChildren() => new[] { _parser };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (find == _parser && replace is IParser<TInput, TMiddle> realReplace)
                return new TransformParser<TInput, TMiddle, TOutput>(realReplace, _transform);
            return this;
        }

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}