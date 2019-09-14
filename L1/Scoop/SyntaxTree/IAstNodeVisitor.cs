namespace Scoop.SyntaxTree
{
    public interface IAstNodeVisitor
    {
        AstNode Visit(AstNode node);
    }

    public interface IAstNodeVisitorImplementation
    {
        AstNode VisitArrayInitializer(ArrayInitializerNode n);
        AstNode VisitArrayType(ArrayTypeNode n);
        AstNode VisitAttribute(AttributeNode n);
        AstNode VisitCast(CastNode n);
        AstNode VisitCompilationUnit(CompilationUnitNode n);
        AstNode VisitConst(ConstNode n);
        AstNode VisitChar(CharNode n);
        AstNode VisitClass(ClassNode n);
        AstNode VisitConditional(ConditionalNode n);
        AstNode VisitConstructor(ConstructorNode n);
        AstNode VisitCSharp(CSharpNode n);
        AstNode VisitDecimal(DecimalNode n);
        AstNode VisitDelegate(DelegateNode n);
        AstNode VisitDottedIdentifier(DottedIdentifierNode n);
        AstNode VisitDouble(DoubleNode n);
        AstNode VisitEmpty(EmptyNode n);
        AstNode VisitEnum(EnumNode n);
        AstNode VisitEnumMember(EnumMemberNode n);
        AstNode VisitField(FieldNode n);
        AstNode VisitFloat(FloatNode n);
        AstNode VisitIdentifier(IdentifierNode n);
        AstNode VisitIndex(IndexNode n);
        AstNode VisitInfixOperation(InfixOperationNode n);
        AstNode VisitInteger(IntegerNode n);
        AstNode VisitInterface(InterfaceNode n);
        AstNode VisitInvoke(InvokeNode n);
        AstNode VisitKeyValueInitializer(KeyValueInitializerNode n);
        AstNode VisitKeyword(KeywordNode n);
        AstNode VisitLambda(LambdaNode n);
        AstNode VisitList<TNode>(ListNode<TNode> n)
            where TNode : AstNode;
        AstNode VisitLong(LongNode n);
        AstNode VisitMemberAccess(MemberAccessNode n);
        AstNode VisitMethod(MethodNode n);
        AstNode VisitMethodDeclare(MethodDeclareNode n);
        AstNode VisitNamedArgument(NamedArgumentNode n);
        AstNode VisitNamespace(NamespaceNode n);
        AstNode VisitNew(NewNode n);
        AstNode VisitOperator(OperatorNode n);
        AstNode VisitParameter(ParameterNode n);
        AstNode VisitPostfixOperation(PostfixOperationNode n);
        AstNode VisitPrefixOperation(PrefixOperationNode n);
        AstNode VisitPropertyInitializer(PropertyInitializerNode n);
        AstNode VisitReturn(ReturnNode n);
        AstNode VisitString(StringNode n);
        AstNode VisitType(TypeNode n);
        AstNode VisitTypeCoerce(TypeCoerceNode n);
        AstNode VisitTypeConstraint(TypeConstraintNode n);
        AstNode VisitUInteger(UIntegerNode n);
        AstNode VisitULong(ULongNode n);
        AstNode VisitUsingDirective(UsingDirectiveNode n);
        AstNode VisitUsingStatement(UsingStatementNode n);
        AstNode VisitVariableDeclare(VariableDeclareNode n);
    }
}