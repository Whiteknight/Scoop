using System.IO;
using System.Text;
using Scoop.SyntaxTree;

namespace Scoop.Transpiler
{
    public partial class CSharpTranspileVisitor
    {
        private readonly OutputRenderer _renderer;

        public CSharpTranspileVisitor(StringBuilder sb)
        {
            var tw = new StringWriter(sb);
            _renderer = new OutputRenderer(tw);
        }

        public CSharpTranspileVisitor(TextWriter writer)
        {
            _renderer = new OutputRenderer(writer);
        }

        public static string ToString(AstNode n)
        {
            var sb = new StringBuilder();
            new CSharpTranspileVisitor(sb).Visit(n);
            return sb.ToString();
        }

        // A couple methods here to delegate to OutputRenderer cleanly

        public void Append(string s) => _renderer.Append(s);

        public void AppendLine(string s = "") => _renderer.AppendLine(s);

        public void AppendLineAndIndent(string s = "") => _renderer.AppendLineAndIndent(s);

        public void DecreaseIndent() => _renderer.DecreaseIndent();

        public void IncreaseIndent() => _renderer.IncreaseIndent();

        public void AppendIndent() => _renderer.AppendIndent();
    }
}
