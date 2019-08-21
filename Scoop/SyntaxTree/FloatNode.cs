﻿using Scoop.Tokenization;

namespace Scoop.SyntaxTree
{
    public class FloatNode : AstNode
    {
        public FloatNode(float value)
        {
            Value = value;
        }

        public FloatNode(Token t)
        {
            Value = float.Parse(t.Value);
            Location = t.Location;
        }

        public float Value { get; set; }

        public override AstNode Accept(AstNodeVisitor visitor) => visitor.VisitFloat(this);
    }
}