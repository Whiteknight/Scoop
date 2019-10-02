using Scoop.SyntaxTree;

namespace Scoop.Parsers.Visiting
{
    public interface IParserVisitor
    {
        IParser Visit(IParser parser);
    }

    public interface IParserVisitorImplementation
    {
        IParser<AstNode> VisitApplyPostfix(ApplyPostfixParser p);
        IParser<TNode> VisitDeferred<TNode>(DeferredParser<TNode> p);
        IParser<TNode> VisitError<TNode>(ErrorParser<TNode> p) 
            where TNode : AstNode, new();
        IParser<TNode> VisitFirst<TNode>(FirstParser<TNode> p) 
            where TNode : AstNode;

        IParser<IdentifierNode> VisitIdentifier(IdentifierParser p);
        IParser<AstNode> VisitInfix(InfixOperatorParser p);
        IParser<KeywordNode> VisitKeyword(KeywordParser p);
        IParser<ListNode<TOutput>> VisitList<TOutput, TItem>(ListParser<TOutput, TItem> p) 
            where TOutput : AstNode;

        IParser<OperatorNode> VisitOperator(OperatorParser p);
        IParser<AstNode> VisitOptional(OptionalParser p);
        IParser<TOutput> VisitProduce<TOutput>(ProduceParser<TOutput> p);
        IParser<TOutput> VisitReplaceable<TOutput>(ReplaceableParser<TOutput> p);
        IParser<TOutput> VisitRequired<TOutput>(RequiredParser<TOutput> p) 
            where TOutput : AstNode;

        IParser<ListNode<TOutput>> VisitSeparatedList<TOutput, TItem>(SeparatedListParser<TOutput, TItem> p) 
            where TOutput : AstNode 
            where TItem : AstNode;

        IParser<TOutput> VisitSequence<TOutput>(SequenceParser<TOutput> p);
        IParser<TOutput> VisitToken<TOutput>(TokenParser<TOutput> p) 
            where TOutput : AstNode;

        IParser<TOutput> VisitTransform<TOutput, TInput>(TransformParser<TOutput, TInput> p) 
            where TOutput : AstNode 
            where TInput : AstNode;
    }
}
