using Scoop.SyntaxTree;

namespace Scoop.Parsers.Visiting
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

        public virtual IParser<AstNode> VisitApplyPostfix(ApplyPostfixParser p)
        {
            return p;
        }

        public virtual IParser<TNode> VisitDeferred<TNode>(DeferredParser<TNode> p)
        {
            return p;
        }

        public virtual IParser<TNode> VisitError<TNode>(ErrorParser<TNode> p) 
            where TNode : AstNode, new()
        {
            return p;
        }

        public virtual IParser<TNode> VisitFirst<TNode>(FirstParser<TNode> p) 
            where TNode : AstNode
        {
            return p;
        }

        public virtual IParser<IdentifierNode> VisitIdentifier(IdentifierParser p)
        {
            return p;
        }

        public virtual IParser<AstNode> VisitInfix(InfixOperatorParser p)
        {
            return p;
        }

        public virtual IParser<KeywordNode> VisitKeyword(KeywordParser p)
        {
            return p;
        }

        public virtual IParser<ListNode<TOutput>> VisitList<TOutput, TItem>(ListParser<TOutput, TItem> p) 
            where TOutput : AstNode
        {
            return p;
        }

        public virtual IParser<OperatorNode> VisitOperator(OperatorParser p)
        {
            return p;
        }

        public virtual IParser<AstNode> VisitOptional(OptionalParser p)
        {
            return p;
        }

        public virtual IParser<TOutput> VisitProduce<TOutput>(ProduceParser<TOutput> p)
        {
            return p;
        }

        public virtual IParser<TOutput> VisitRequired<TOutput>(RequiredParser<TOutput> p) 
            where TOutput : AstNode
        {
            return p;
        }

        public virtual IParser<ListNode<TOutput>> VisitSeparatedList<TOutput, TItem>(SeparatedListParser<TOutput, TItem> p) 
            where TOutput : AstNode 
            where TItem : AstNode
        {
            return p;
        }

        public virtual IParser<TOutput> VisitSequence<TOutput>(SequenceParser<TOutput> p)
        {
            return p;
        }

        public virtual IParser<TOutput> VisitToken<TOutput>(TokenParser<TOutput> p) 
            where TOutput : AstNode
        {
            return p;
        }

        public virtual IParser<TOutput> VisitTransform<TOutput, TInput>(TransformParser<TOutput, TInput> p) 
            where TOutput : AstNode 
            where TInput : AstNode
        {
            return p;
        }
    }
}