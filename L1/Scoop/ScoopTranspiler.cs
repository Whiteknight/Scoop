using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Scoop.Grammar;
using Scoop.Parsers;
using Scoop.SyntaxTree;
using Scoop.Tokenization;
using Scoop.Transpiler;

namespace Scoop
{
    public static class ScoopTranspiler
    {
        public static AstNode ParseFile(string inputFileName)
        {
            using (var source = new StreamCharacterSequence(inputFileName, Encoding.UTF8))
            {
                var tokenizer = new Tokenizer(new TokenScanner(source));
                var ast = new ScoopGrammar().CompilationUnits.Parse(tokenizer).GetResult();
                ast.FileName = inputFileName;
                return ast;
            }
        }

        public static TranspileResult TranspileFile(string inputFileName, string outputFileName = null)
        {
            var ast = ParseFile(inputFileName);
            var validateResults = ast.Validate();
            if (validateResults.Any())
                return TranspileResult.ForValidationFailure(validateResults);

            outputFileName = outputFileName ?? inputFileName + ".cs";
            using (var outStream = new StreamWriter(outputFileName, false))
            {
                new CSharpTranspileVisitor(outStream).Visit(ast);
                outStream.Flush();
            }

            return TranspileResult.ForSuccess(outputFileName);
        }
    }

    public class TranspileResult
    {
        public bool IsSuccess { get; private set; }
        public IReadOnlyList<Diagnostic> Diagnostics { get; private set; }
        public string OutputFileName { get; private set; }

        public static TranspileResult ForValidationFailure(IReadOnlyList<Diagnostic> diagnostics)
        {
            return new TranspileResult { IsSuccess = false, Diagnostics = diagnostics };
        }

        public static TranspileResult ForSuccess(string outFileName)
        {
            return new TranspileResult { IsSuccess = true, OutputFileName = outFileName };
        }
    }
}
