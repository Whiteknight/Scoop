﻿using Scoop.Parsing.Tokenization;
using Scoop.SyntaxTree.Visiting;

namespace Scoop.SyntaxTree
{
    public class LongNode : AstNode
    {
        public LongNode(long value)
        {
            Value = value;
        }

        public LongNode(Token t)
        {
            Value = long.Parse(t.Value);
            Location = t.Location;
        }

        public long Value { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitLong(this);
    }
}