namespace Scoop.SyntaxTree
{
    public abstract class AstNodeVisitor
    {
        public AstNode Visit(AstNode node) => node?.Accept(this);

        public virtual AstNode VisitArrayType(ArrayTypeNode n)
        {
            Visit(n.ElementType);
            return n;
        }

        public virtual AstNode VisitCast(CastNode n)
        {
            Visit(n.Type);
            Visit(n.Right);
            return n;
        }

        public virtual AstNode VisitCompilationUnit(CompilationUnitNode n)
        {
            foreach (var ud in n.UsingDirectives.OrEmptyIfNull())
                Visit(ud);
            foreach (var ns in n.Namespaces.OrEmptyIfNull())
                Visit(ns);
            return n;
        }

        public virtual AstNode VisitChar(CharNode n) => n;

        public virtual AstNode VisitChildType(ChildTypeNode n)
        {
            Visit(n.Parent);
            Visit(n.Child);
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

        public virtual AstNode VisitDecimal(DecimalNode n) => n;

        public virtual AstNode VisitDottedIdentifier(DottedIdentifierNode n) => n;

        public virtual AstNode VisitDouble(DoubleNode n) => n;

        public virtual AstNode VisitFloat(FloatNode n) => n;

        public virtual AstNode VisitIdentifier(IdentifierNode n) => n;

        public virtual AstNode VisitInfixOperation(InfixOperationNode n)
        {
            Visit(n.Left);
            Visit(n.Operator);
            Visit(n.Right);
            return n;
        }

        public virtual AstNode VisitInteger(IntegerNode n) => n;

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

        public virtual AstNode VisitInvoke(InvokeNode n)
        {
            Visit(n.Instance);
            foreach (var a in n.Arguments.OrEmptyIfNull())
                Visit(a);
            return n;
        }

        public virtual AstNode VisitKeyword(KeywordNode n) => n;

        public virtual AstNode VisitLong(LongNode n) => n;

        public virtual AstNode VisitMemberAccess(MemberAccessNode n)
        {
            Visit(n.Instance);
            Visit(n.MemberName);
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

        public virtual AstNode VisitNew(NewNode n)
        {
            Visit(n.Type);
            foreach (var a in n.Arguments.OrEmptyIfNull())
                Visit(a);
            return n;
        }

        public virtual AstNode VisitOperator(OperatorNode n) => n;

        public virtual AstNode VisitParenthesis<TNode>(ParenthesisNode<TNode> n)
            where TNode : AstNode
        {
            Visit(n.Expression);
            return n;
        }

        public virtual AstNode VisitPostfixOperation(PostfixOperationNode n)
        {
            Visit(n.Left);
            Visit(n.Operator);
            return n;
        }

        public virtual AstNode VisitPrefixOperation(PrefixOperationNode n)
        {
            Visit(n.Operator);
            Visit(n.Right);
            return n;
        }

        

        public virtual AstNode VisitReturn(ReturnNode n)
        {
            Visit(n.Expression);
            return n;
        }

        public virtual AstNode VisitString(StringNode n) => n;

        public virtual AstNode VisitType(TypeNode n)
        {
            Visit(n.Name);
            foreach (var a in n.GenericArguments.OrEmptyIfNull())
                Visit(a);
            return n;
        }

        public virtual AstNode VisitUInteger(UIntegerNode n) => n;

        public virtual AstNode VisitULong(ULongNode n) => n;

        public virtual AstNode VisitUsingDirective(UsingDirectiveNode n)
        {
            Visit(n.Alias);
            Visit(n.Namespace);
            return n;
        }

        public virtual AstNode VisitVariableDeclare(VariableDeclareNode n)
        {
            Visit(n.Name);
            return n;
        }
    }
}
