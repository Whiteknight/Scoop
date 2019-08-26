﻿namespace Scoop.SyntaxTree
{
    public interface IAstNodeVisitor
    {
        AstNode Visit(AstNode node);
    }

    public interface IAstNodeVisitorImplementation
    {
        AstNode VisitArrayType(ArrayTypeNode n);
        AstNode VisitCast(CastNode n);
        AstNode VisitCompilationUnit(CompilationUnitNode n);
        AstNode VisitChar(CharNode n);
        AstNode VisitChildType(ChildTypeNode n);
        AstNode VisitClass(ClassNode n);
        AstNode VisitConstructor(ConstructorNode n);
        AstNode VisitDecimal(DecimalNode n);
        AstNode VisitDottedIdentifier(DottedIdentifierNode n);
        AstNode VisitDouble(DoubleNode n);
        AstNode VisitFloat(FloatNode n);
        AstNode VisitIdentifier(IdentifierNode n);
        AstNode VisitInfixOperation(InfixOperationNode n);
        AstNode VisitInteger(IntegerNode n);
        AstNode VisitInterface(InterfaceNode n);
        AstNode VisitInvoke(InvokeNode n);
        AstNode VisitKeyword(KeywordNode n);
        AstNode VisitLong(LongNode n);
        AstNode VisitMemberAccess(MemberAccessNode n);
        AstNode VisitMethod(MethodNode n);
        AstNode VisitNamespace(NamespaceNode n);
        AstNode VisitNew(NewNode n);
        AstNode VisitOperator(OperatorNode n);

        AstNode VisitParenthesis<TNode>(ParenthesisNode<TNode> n)
            where TNode : AstNode;

        AstNode VisitPostfixOperation(PostfixOperationNode n);
        AstNode VisitPrefixOperation(PrefixOperationNode n);
        AstNode VisitReturn(ReturnNode n);
        AstNode VisitString(StringNode n);
        AstNode VisitType(TypeNode n);
        AstNode VisitUInteger(UIntegerNode n);
        AstNode VisitULong(ULongNode n);
        AstNode VisitUsingDirective(UsingDirectiveNode n);
        AstNode VisitVariableDeclare(VariableDeclareNode n);
    }
}