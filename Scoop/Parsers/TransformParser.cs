using System;
using System.Collections.Generic;
using System.Linq;
using Scoop.Parsers.Visiting;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    /// <summary>
    /// Transforms the output of one parser into a different form based on context
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TInput"></typeparam>
    public class TransformParser<TOutput, TInput> : IParser<TOutput>
        where TOutput : AstNode
        where TInput : AstNode
    {
        private readonly IParser<TInput> _parser;
        private readonly Func<TInput, TOutput> _transform;

        public TransformParser(IParser<TInput> parser, Func<TInput, TOutput> transform)
        {
            _parser = parser;
            _transform = transform;
        }

        public TOutput TryParse(ITokenizer t)
        {
            var result = _parser.Parse(t);
            if (!result.IsSuccess)
                return default;
            return _transform(result.Value);
        }

        public string Name { get; set; }

        public IParser Accept(IParserVisitorImplementation visitor) => visitor.VisitTransform(this);

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}