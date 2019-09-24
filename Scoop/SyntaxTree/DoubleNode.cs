﻿using Scoop.SyntaxTree.Visiting;
using Scoop.Tokenization;

namespace Scoop.SyntaxTree
{
    public class DoubleNode : AstNode
    {
        public DoubleNode(double value)
        {
            Value = value;
        }

        public DoubleNode(Token t)
        {
            Value = double.Parse(t.Value);
            Location = t.Location;
        }

        public double Value { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitDouble(this);
    }
}