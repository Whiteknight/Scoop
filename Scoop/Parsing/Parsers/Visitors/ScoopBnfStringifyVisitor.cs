using System.Linq;
using System.Text;
using ParserObjects.Parsers.Visitors;

namespace Scoop.Parsing.Parsers.Visitors
{
    public class ScoopBnfStringifyVisitor : BnfStringifyVisitor
    {
        public ScoopBnfStringifyVisitor(StringBuilder sb) : base(sb)
        {
        }

        public void VisitTyped(InfixOperatorParser p)
        {
            var children = p.GetChildren().ToArray();
            VisitChild(children[0]);
            _current.Append(" ");
            VisitChild(children[1]);
            _current.Append(" ");
            VisitChild(children[2]);
        }
    }
}
