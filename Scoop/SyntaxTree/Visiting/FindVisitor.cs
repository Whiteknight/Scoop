using System;
using System.Collections.Generic;

namespace Scoop.SyntaxTree.Visiting
{
    public class FindVisitor : AstNodeVisitorBase
    {
        private readonly Func<AstNode, bool> _predicate;
        private readonly bool _justOne;
        private readonly List<AstNode> _nodes;
        private bool _canStop;

        private FindVisitor(Func<AstNode, bool> predicate, bool justOne)
        {
            _predicate = predicate;
            _justOne = justOne;
            _nodes = new List<AstNode>();
            _canStop = false;
        }


        public override AstNode Visit(AstNode node)
        {
            if (_canStop)
                return node;

            if (_predicate(node))
            {
                _nodes.Add(node);
                if (_justOne)
                    _canStop = true;
            }

            return base.Visit(node);
        }
    }
}
