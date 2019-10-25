﻿using Scoop.Parsing.Tokenization;
using Scoop.SyntaxTree.Visiting;

namespace Scoop.SyntaxTree
{
    public class ULongNode : AstNode
    {
        public ULongNode(ulong value)
        {
            Value = value;
        }

        public ULongNode(Token t)
        {
            Value = ulong.Parse(t.Value);
            Location = t.Location;
        }

        public ulong Value { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitULong(this);
    }
}