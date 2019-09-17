namespace Scoop.SyntaxTree
{
    public abstract class AstNodeVisitorBase : IAstNodeVisitor, IAstNodeVisitorImplementation
    {
        public virtual AstNode Visit(AstNode node) => node?.Accept(this);

        public virtual AstNode VisitArrayInitializer(ArrayInitializerNode n)
        {
            Visit(n.Key);
            Visit(n.Value);
            return n;
        }

        public virtual AstNode VisitArrayType(ArrayTypeNode n)
        {
            return n;
        }

        public virtual AstNode VisitAttribute(AttributeNode n)
        {
            Visit(n.Target);
            Visit(n.Type);
            Visit(n.Arguments);
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
            Visit(n.Members);
            return n;
        }

        public virtual AstNode VisitConst(ConstNode n)
        {
            Visit(n.AccessModifier);
            Visit(n.Type);
            Visit(n.Name);
            Visit(n.Value);
            return n;
        }

        public virtual AstNode VisitChar(CharNode n)
        {
            return n;
        }

        public virtual AstNode VisitClass(ClassNode n)
        {
            Visit(n.Attributes);
            Visit(n.AccessModifier);
            Visit(n.Modifiers);
            Visit(n.Type);
            Visit(n.Name);
            Visit(n.GenericTypeParameters);
            Visit(n.Interfaces);
            Visit(n.TypeConstraints);
            Visit(n.Members);
            return n;
        }

        public virtual AstNode VisitConditional(ConditionalNode n)
        {
            Visit(n.Condition);
            Visit(n.IfTrue);
            Visit(n.IfFalse);
            return n;
        }

        public virtual AstNode VisitConstructor(ConstructorNode n)
        {
            Visit(n.Attributes);
            Visit(n.ClassName);
            Visit(n.AccessModifier);
            Visit(n.Parameters);
            Visit(n.ThisArgs);
            Visit(n.Statements);
            return n;
        }

        public virtual AstNode VisitCSharp(CSharpNode n)
        {
            return n;
        }

        public virtual AstNode VisitDecimal(DecimalNode n)
        {
            return n;
        }

        public virtual AstNode VisitDelegate(DelegateNode n)
        {
            Visit(n.Attributes);
            Visit(n.AccessModifier);
            Visit(n.ReturnType);
            Visit(n.Name);
            Visit(n.Parameters);
            Visit(n.GenericTypeParameters);
            Visit(n.TypeConstraints);
            return n;
        }

        public virtual AstNode VisitDottedIdentifier(DottedIdentifierNode n)
        {
            return n;
        }

        public virtual AstNode VisitDouble(DoubleNode n)
        {
            return n;
        }

        public virtual AstNode VisitEmpty(EmptyNode n)
        {
            return n;
        }

        public virtual AstNode VisitEnum(EnumNode n)
        {
            Visit(n.Attributes);
            Visit(n.AccessModifier);
            Visit(n.Name);
            Visit(n.Members);
            return n;
        }

        public virtual AstNode VisitEnumMember(EnumMemberNode n)
        {
            Visit(n.Attributes);
            Visit(n.Name);
            Visit(n.Value);
            return n;
        }

        public virtual AstNode VisitField(FieldNode n)
        {
            Visit(n.Attributes);
            Visit(n.Type);
            Visit(n.Name);
            return n;
        }

        public virtual AstNode VisitFloat(FloatNode n)
        {
            return n;
        }

        public virtual AstNode VisitIdentifier(IdentifierNode n)
        {
            return n;
        }

        public virtual AstNode VisitIndex(IndexNode n)
        {
            Visit(n.Instance);
            Visit(n.Arguments);
            return n;
        }

        public virtual AstNode VisitInfixOperation(InfixOperationNode n)
        {
            Visit(n.Left);
            Visit(n.Operator);
            Visit(n.Right);
            return n;
        }

        public virtual AstNode VisitInteger(IntegerNode n)
        {
            return n;
        }

        public virtual AstNode VisitInterface(InterfaceNode n)
        {
            Visit(n.Attributes);
            Visit(n.AccessModifier);
            Visit(n.Name);
            Visit(n.GenericTypeParameters);
            Visit(n.Interfaces);
            Visit(n.TypeConstraints);
            Visit(n.Members);
            return n;
        }

        public virtual AstNode VisitInvoke(InvokeNode n)
        {
            Visit(n.Instance);
            Visit(n.Arguments);
            return n;
        }

        public virtual AstNode VisitKeyValueInitializer(KeyValueInitializerNode n)
        {
            Visit(n.Key);
            Visit(n.Value);
            return n;
        }

        public virtual AstNode VisitKeyword(KeywordNode n)
        {
            return n;
        }

        public virtual AstNode VisitLambda(LambdaNode n)
        {
            Visit(n.Parameters);
            Visit(n.Statements);
            return n;
        }

        public virtual AstNode VisitList<TNode>(ListNode<TNode> n) where TNode : AstNode
        {
            Visit(n.Separator);
            if (n.Items != null)
            {
                foreach (var i in n.Items)
                    Visit(i);
            }

            return n;
        }

        public virtual AstNode VisitLong(LongNode n)
        {
            return n;
        }

        public virtual AstNode VisitMemberAccess(MemberAccessNode n)
        {
            Visit(n.Instance);
            Visit(n.MemberName);
            Visit(n.GenericArguments);
            return n;
        }

        public virtual AstNode VisitMethod(MethodNode n)
        {
            Visit(n.Attributes);
            Visit(n.AccessModifier);
            Visit(n.Modifiers);
            Visit(n.ReturnType);
            Visit(n.Name);
            Visit(n.Parameters);
            Visit(n.TypeConstraints);
            Visit(n.Statements);
            Visit(n.GenericTypeParameters);
            return n;
        }

        public virtual AstNode VisitMethodDeclare(MethodDeclareNode n)
        {
            Visit(n.Attributes);
            Visit(n.ReturnType);
            Visit(n.Name);
            Visit(n.Parameters);
            Visit(n.GenericTypeParameters);
            Visit(n.TypeConstraints);
            return n;
        }

        public virtual AstNode VisitNamedArgument(NamedArgumentNode n)
        {
            Visit(n.Name);
            Visit(n.Separator);
            Visit(n.Value);
            return n;
        }

        public virtual AstNode VisitNamespace(NamespaceNode n)
        {
            Visit(n.Attributes);
            Visit(n.Name);
            Visit(n.Declarations);
            return n;
        }

        public virtual AstNode VisitNew(NewNode n)
        {
            Visit(n.Type);
            Visit(n.Arguments);
            Visit(n.Initializers);
            return n;
        }

        public virtual AstNode VisitOperator(OperatorNode n)
        {
            return n;
        }

        public virtual AstNode VisitParameter(ParameterNode n)
        {
            Visit(n.Attributes);
            Visit(n.Type);
            Visit(n.Name);
            Visit(n.DefaultValue);
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

        public virtual AstNode VisitPropertyInitializer(PropertyInitializerNode n)
        {
            Visit(n.Property);
            Visit(n.Value);
            return n;
        }

        public virtual AstNode VisitReturn(ReturnNode n)
        {
            Visit(n.Expression);
            return n;
        }

        public virtual AstNode VisitString(StringNode n)
        {
            return n;
        }

        public virtual AstNode VisitType(TypeNode n)
        {
            Visit(n.Name);
            Visit(n.GenericArguments);
            Visit(n.ArrayTypes);
            Visit(n.Child);
            return n;
        }

        public AstNode VisitTypeCoerce(TypeCoerceNode n)
        {
            Visit(n.Left);
            Visit(n.Operator);
            Visit(n.Type);
            Visit(n.Alias);
            return n;
        }

        public virtual AstNode VisitTypeConstraint(TypeConstraintNode n)
        {
            Visit(n.Type);
            Visit(n.Constraints);
            return n;
        }

        public virtual AstNode VisitUInteger(UIntegerNode n)
        {
            return n;
        }

        public virtual AstNode VisitULong(ULongNode n)
        {
            return n;
        }

        public virtual AstNode VisitUsingDirective(UsingDirectiveNode n)
        {
            Visit(n.Alias);
            Visit(n.Namespace);
            return n;
        }

        public virtual AstNode VisitUsingStatement(UsingStatementNode n)
        {
            Visit(n.Disposable);
            Visit(n.Statement);
            return n;
        }

        public virtual AstNode VisitVariableDeclare(VariableDeclareNode n)
        {
            Visit(n.Type);
            Visit(n.Name);
            Visit(n.Value);
            return n;
        }
    }
}
