using System;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
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
        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}