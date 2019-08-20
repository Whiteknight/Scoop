namespace Scoop.SyntaxTree
{
    public abstract class AstNodeVisitor
    {
        public AstNode Visit(AstNode node) => node?.Accept(this);

        public virtual AstNode VisitCompilationUnit(CompilationUnitNode n)
        {
            foreach (var ud in n.UsingDirectives.OrEmptyIfNull())
                Visit(ud);
            foreach (var ns in n.Namespaces.OrEmptyIfNull())
                Visit(ns);
            return n;
        }

        public virtual AstNode VisitClass(ClassNode n)
        {
            Visit(n.AccessModifier);
            Visit(n.Name);
            foreach (var i in n.Interfaces.OrEmptyIfNull())
                Visit(i);
            foreach (var m in n.Members.OrEmptyIfNull())
                Visit(m);
            return n;
        }

        public virtual AstNode VisitConstructor(ConstructorNode n)
        {
            Visit(n.AccessModifier);
            foreach (var p in n.Parameters.OrEmptyIfNull())
                Visit(p);
            foreach (var s in n.Statements.OrEmptyIfNull())
                Visit(s);
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
            Visit(n.AccessModifier);
            Visit(n.Name);
            foreach (var i in n.Interfaces.OrEmptyIfNull())
                Visit(i);
            foreach (var m in n.Members.OrEmptyIfNull())
                Visit(m);
            return n;
        }

        public virtual AstNode VisitKeyword(KeywordNode n)
        {
            return n;
        }

        public virtual AstNode VisitMethod(MethodNode n)
        {
            Visit(n.AccessModifier);
            Visit(n.ReturnType);
            foreach (var p in n.Parameters.OrEmptyIfNull())
                Visit(p);
            foreach (var s in n.Statements.OrEmptyIfNull())
                Visit(s);
            return n;
        }

        public virtual AstNode VisitNamespace(NamespaceNode n)
        {
            Visit(n.Name);
            foreach (var d in n.Declarations.OrEmptyIfNull())
                Visit(d);
            return n;
        }

        public virtual AstNode VisitUsingDirective(UsingDirectiveNode n)
        {
            Visit(n.Alias);
            Visit(n.Namespace);
            return n;
        }
    }
}
