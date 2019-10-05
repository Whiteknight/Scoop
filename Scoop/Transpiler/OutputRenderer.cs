using System.IO;

namespace Scoop.Transpiler
{
    public class OutputRenderer
    {
        private readonly TextWriter _tw;
        private int _indent;

        public OutputRenderer(TextWriter tw)
        {
            _tw = tw;
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