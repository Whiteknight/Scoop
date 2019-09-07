using System;
using System.Collections.Generic;
using System.Text;

namespace Scoop.SyntaxTree
{
    public class TypeConstraintNode : AstNode
    {
        public IdentifierNode Type { get; set; }
        public List<AstNode> Constraints { get; set; }
        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitTypeConstraint(this);
    }
}
