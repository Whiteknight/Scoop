using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Scoop.Grammar;
using Scoop.SyntaxTree;
using Scoop.Tokenization;
using Scoop.Transpiler;

namespace Scoop
{
    public class ScoopTranspiler
    {
        private ScoopGrammar _grammar;

        public ScoopTranspiler()
        {
            _grammar = new ScoopGrammar();
        }

        public void AdjustGrammar(Func<ScoopGrammar, ScoopGrammar> adjust)
        {
            _grammar = adjust?.Invoke(_grammar);
            if (_grammar == null)
                throw new InvalidOperationException("Cannot have a null grammar");
        }

        public TranspileResult TranspileFile(string inputFileName, string outputFileName = null)
        {
            CompilationUnitNode ast;

            using (var source = new StreamCharacterSequence(inputFileName, Encoding.UTF8))
            {
                var tokenizer = new Tokenizer(new TokenScanner(source));
                var result = _grammar.CompilationUnits.Parse(tokenizer);
                ast = result.Value;
                ast.FileName = inputFileName;
            }
            var validateResults = ast.Validate();
            if (validateResults.Any())
                return TranspileResult.ForValidationFailure(validateResults);

            outputFileName = outputFileName ?? inputFileName + ".cs";
            using (var outStream = new StreamWriter(outputFileName, false))
            {
                var preamble = Formatting.GetGeneratedFilePreamble("Scoop", inputFileName);
                outStream.WriteLine(preamble);
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
