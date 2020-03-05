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
    }
}
