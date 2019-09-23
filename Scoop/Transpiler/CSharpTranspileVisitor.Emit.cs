using System.IO;
using System.Text;
using Scoop.SyntaxTree;

namespace Scoop.Transpiler
{
    public partial class CSharpTranspileVisitor
    {
        private readonly TextWriter _tw;
        private int _indent;

        public CSharpTranspileVisitor(StringBuilder sb)
        {
            _tw = new StringWriter(sb);
            _indent = 0;
        }

        public CSharpTranspileVisitor(TextWriter writer)
        {
            _tw = writer;
            _indent = 0;
        }

        public static string ToString(AstNode n)
        {
            var sb = new StringBuilder();
            new CSharpTranspileVisitor(sb).Visit(n);
            return sb.ToString();
        }

        public void Append(string s) => _tw.Write(s);

        public void AppendLine(string s = "") => _tw.WriteLine(s);

        public void AppendLineAndIndent(string s = "")
        {
            AppendLine(s);
            AppendIndent();
        }

        public void DecreaseIndent() => _indent--;
        public void IncreaseIndent() => _indent++;
        public void AppendIndent()
        {
            if (_indent <= 0)
                return;
            _tw.Write(new string(' ', _indent * 4));
        }
    }
}
