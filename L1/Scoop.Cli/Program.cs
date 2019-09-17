using System.IO;
using System.Linq;
using System.Text;
using Scoop.Tokenization;
using Scoop.Transpiler;

namespace Scoop.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: Option to invoke roslyn and generate the .dll or .exe?
            // TODO: Option to dump to console instead of write to outfile?
            // TODO: Option to read from console?
            // TODO: compile all args, and handle wildcards?
            var fileName = args.First();
            var source = new StreamCharacterSequence(fileName, Encoding.UTF8);
            var tokenizer = new Tokenizer(new TokenScanner(source));
            var ast = new ScoopGrammar().CompilationUnits.Parse(tokenizer).GetResult();
            var outFile = fileName + ".cs";
            var outStream = new StreamWriter(outFile, false);
            new CSharpTranspileVisitor(outStream).Visit(ast);
            outStream.Flush();
            outStream.Dispose();
            source.Dispose();
        }
    }
}
