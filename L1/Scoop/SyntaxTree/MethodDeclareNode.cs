﻿using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class MethodDeclareNode : AstNode, IHasAttributes
    {
        // Represents a declaration of a method in an interface

        public List<AttributeNode> Attributes { get; set; }
        public AstNode ReturnType { get; set; }
        public IdentifierNode Name { get; set; }
        public List<ParameterNode> Parameters { get; set; }
        public List<AstNode> GenericTypeParameters { get; set; }
        public List<TypeConstraintNode> TypeConstraints { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitMethodDeclare(this);
    }
}