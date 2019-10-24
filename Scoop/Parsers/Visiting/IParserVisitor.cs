using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop.Parsers.Visiting
{
    public interface IParserVisitor
    {
        IParser Visit(IParser parser);
    }

    public interface IParserVisitorImplementation
    {
        IParser<Token, AstNode> VisitApplyPostfix(ApplyPostfixParser p);
        IParser<TInput, TOutput> VisitDeferred<TInput, TOutput>(DeferredParser<TInput, TOutput> p);
        IParser<Token, TOutput> VisitError<TOutput>(ErrorParser<TOutput> p) 
            where TOutput : AstNode, new();
        IParser<TInput, TOutput> VisitFirst<TInput, TOutput>(FirstParser<TInput, TOutput> p);

        IParser<Token, AstNode> VisitInfix(InfixOperatorParser p);

        IParser<TInput, TOutput> VisitList<TInput, TItem, TOutput>(ListParser<TInput, TItem, TOutput> p);

        IParser<TInput, TOutput> VisitOptional<TInput, TOutput>(OptionalParser<TInput, TOutput> p);
        IParser<TInput, TOutput> VisitPredicate<TInput, TOutput>(PredicateParser<TInput, TOutput> p);
        IParser<TInput, TOutput> VisitProduce<TInput, TOutput>(ProduceParser<TInput, TOutput> p);
        IParser<TInput, TOutput> VisitReplaceable<TInput, TOutput>(ReplaceableParser<TInput, TOutput> p);
        IParser<TInput, TOutput> VisitRequired<TInput, TOutput>(RequiredParser<TInput, TOutput> p) ;

        IParser<TInput, TOutput> VisitSeparatedList<TInput, TItem, TSeparator, TOutput>(SeparatedListParser<TInput, TItem, TSeparator, TOutput> p) 
            where TOutput : AstNode;

        IParser<TInput, TOutput> VisitSequence<TInput, TOutput>(SequenceParser<TInput, TOutput> p);

        IParser<TInput, TOutput> VisitTransform<TInput, TMiddle, TOutput>(TransformParser<TInput, TMiddle, TOutput> p);
    }
}
