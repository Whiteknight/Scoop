using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scoop.Parsing.Tokenization;
using Scoop.SyntaxTree;

namespace Scoop.Parsing.Parsers.Visitors
{
    public class GrammarStringifyVisitor : IParserVisitor, IParserVisitorImplementation
    {
        private readonly StringBuilder _builder;
        private readonly Stack<StringBuilder> _history;
        private readonly HashSet<IParser> _seen;
        private StringBuilder _current;

        public GrammarStringifyVisitor(StringBuilder sb)
        {
            _builder = sb;
            _history = new Stack<StringBuilder>();
            _seen = new HashSet<IParser>();
        }

        public IParser Visit(IParser parser)
        {
            // Top-level sb, we'll be throwing this away
            _current = new StringBuilder();
            VisitChild(parser);
            return parser;
        }

        private void VisitChild(IParser parser)
        {
            if (parser == null)
                return;
            if (_seen.Contains(parser))
            {
                if (string.IsNullOrEmpty(parser.Name))
                    _current.Append("<ALREADY SEEN UNNAMED PARSER>");
                else
                {
                    _current.Append("<");
                    _current.Append(parser.Name);
                    _current.Append(">");
                }

                return;
            }

            _seen.Add(parser);

            if (string.IsNullOrEmpty(parser.Name))
            {
                parser.Accept(this);
                return;
            }

            // 1. Append a tag to the current builder
            _current.Append("<");
            _current.Append(parser.Name);
            _current.Append(">");

            // 2. Start a new builder to write out the child rule
            _history.Push(_current);
            _current = new StringBuilder();
            parser.Accept(this);

            // 3. Write the child rule to the overall builder, then pop it
            var rule = _current.ToString();
            if (!string.IsNullOrEmpty(rule))
            {
                _builder.Append(parser.Name);
                _builder.Append(" = ");
                _builder.AppendLine(_current.ToString());
            }

            _current = _history.Pop();
        }

        public IParser<TInput, TOutput> VisitApplyPostfix<TInput, TOutput>(ApplyPostfixParser<TInput, TOutput> p)
        {
            _current.Append("POSTFIX");
            return p;
        }

        public IParser<TInput, TOutput> VisitDeferred<TInput, TOutput>(DeferredParser<TInput, TOutput> p)
        {
            VisitChild(p.GetChildren().First());
            return p;
        }

        public IParser<TInput, TOutput> VisitFirst<TInput, TOutput>(FirstParser<TInput, TOutput> p)
        {
            var children = p.GetChildren();
            _current.Append("(");
            VisitChild(children.First());

            foreach (var child in children.Skip(1))
            {
                _current.Append(" | ");
                VisitChild(child);
            }
            _current.Append(")");

            return p;
        }

        public IParser<Token, AstNode> VisitInfix(InfixOperatorParser p)
        {
            var children = p.GetChildren().ToArray();
            VisitChild(children[0]);
            _current.Append(" ");
            VisitChild(children[1]);
            _current.Append(" ");
            VisitChild(children[2]);
            return p;
        }

        public IParser<TInput, TOutput> VisitList<TInput, TItem, TOutput>(ListParser<TInput, TItem, TOutput> p)
        {
            VisitChild(p.GetChildren().First());
            _current.Append(p.AtLeastOne ? "+" : "*");
            return p;
        }

        public IParser<TInput, TOutput> VisitOptional<TInput, TOutput>(OptionalParser<TInput, TOutput> p)
        {
            VisitChild(p.GetChildren().First());
            _current.Append("?");
            return p;
        }

        public IParser<TInput, TOutput> VisitPredicate<TInput, TOutput>(PredicateParser<TInput, TOutput> p)
        {
            //_current.Append("PREDICATE");
            return p;
        }

        public IParser<TInput, TOutput> VisitProduce<TInput, TOutput>(ProduceParser<TInput, TOutput> p)
        {
            _current.Append("PRODUCE");
            return p;
        }

        public IParser<TInput, TOutput> VisitReplaceable<TInput, TOutput>(ReplaceableParser<TInput, TOutput> p)
        {
            VisitChild(p.GetChildren().First());
            return p;
        }

        public IParser<TInput, TOutput> VisitRequired<TInput, TOutput>(RequiredParser<TInput, TOutput> p)
        {
            VisitChild(p.GetChildren().First());
            return p;
        }

        public IParser<TInput, TOutput> VisitSequence<TInput, TOutput>(RuleParser<TInput, TOutput> p)
        {
            _current.Append("(");
            var children = p.GetChildren().ToArray();
            VisitChild(children[0]);
            foreach (var child in children.Skip(1))
            {
                _current.Append(" ");
                VisitChild(child);
            }

            _current.Append(")");
            return p;
        }

        public IParser<TInput, TOutput> VisitTransform<TInput, TMiddle, TOutput>(TransformParser<TInput, TMiddle, TOutput> p)
        {
            VisitChild(p.GetChildren().First());
            return p;
        }
    }
}
