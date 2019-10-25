using Scoop.Parsing.Tokenization;
using Scoop.SyntaxTree;

namespace Scoop.Parsing.Parsers.Visitors
{
    public abstract class ParserVisitorBase : IParserVisitor, IParserVisitorImplementation
    {
        public virtual IParser Visit(IParser parser)
        {
            parser = parser.Accept(this);
            foreach (var child in parser.GetChildren())
            {
                var newChild = Visit(child);
                if (newChild != child)
                    parser = parser.ReplaceChild(child, newChild);
            }

            return parser;
        }

        public virtual IParser<TInput, TOutput> VisitApplyPostfix<TInput, TOutput>(ApplyPostfixParser<TInput, TOutput> p)
        {
            return p;
        }

        public virtual IParser<TInput, TOutput> VisitDeferred<TInput, TOutput>(DeferredParser<TInput, TOutput> p)
        {
            return p;
        }

        public virtual IParser<TInput, TOutput> VisitFirst<TInput, TOutput>(FirstParser<TInput, TOutput> p)
        {
            return p;
        }

        public virtual IParser<Token, AstNode> VisitInfix(InfixOperatorParser p)
        {
            return p;
        }

        public virtual IParser<TInput, TOutput> VisitList<TInput, TItem, TOutput>(ListParser<TInput, TItem, TOutput> p)
        {
            return p;
        }

        public virtual IParser<TInput, TOutput> VisitOptional<TInput, TOutput>(OptionalParser<TInput, TOutput> p)
        {
            return p;
        }

        public virtual IParser<TInput, TOutput> VisitPredicate<TInput, TOutput>(PredicateParser<TInput, TOutput> p)
        {
            return p;
        }

        public virtual IParser<TInput, TOutput> VisitProduce<TInput, TOutput>(ProduceParser<TInput, TOutput> p)
        {
            return p;
        }

        public virtual IParser<TInput, TOutput> VisitReplaceable<TInput, TOutput>(ReplaceableParser<TInput, TOutput> p)
        {
            return p;
        }

        public virtual IParser<TInput, TOutput> VisitRequired<TInput, TOutput>(RequiredParser<TInput, TOutput> p)
        {
            return p;
        }

        public virtual IParser<TInput, TOutput> VisitSequence<TInput, TOutput>(RuleParser<TInput, TOutput> p)
        {
            return p;
        }

        public virtual IParser<TInput, TOutput> VisitTransform<TInput, TMiddle, TOutput>(TransformParser<TInput, TMiddle, TOutput> p)
        {
            return p;
        }
    }
}