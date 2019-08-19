namespace Scoop.SyntaxTree
{
    public abstract partial class AstNodeVisitor
    {
        public AstNode Visit(AstNode node) => node.Accept(this);

        public virtual AstNode VisitCompilationUnit(CompilationUnitNode n)
        {
            return n;
        }

        public virtual AstNode VisitClass(ClassNode n)
        {
            return n;
        }

        public virtual AstNode VisitConstructor(ConstructorNode n)
        {
            return n;
        }

        public virtual AstNode VisitDottedIdentifier(DottedIdentifierNode n)
        {
            return n;
        }

        public virtual AstNode VisitIdentifier(IdentifierNode n)
        {
            return n;
        }

        public virtual AstNode VisitInterface(InterfaceNode n)
        {
            return n;
        }

        public virtual AstNode VisitKeyword(KeywordNode n)
        {
            return n;
        }

        public virtual AstNode VisitMethod(MethodNode n)
        {
            return n;
        }

        public virtual AstNode VisitNamespace(NamespaceNode n)
        {
            return n;
        }

        public virtual AstNode VisitUsingDirective(UsingDirectiveNode n)
        {
            return n;
        }
    }

}
