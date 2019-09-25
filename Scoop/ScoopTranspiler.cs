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
        private static readonly IReadOnlyDictionary<LayerType, Layer> _layers = new Dictionary<LayerType, Layer>
        {
            { LayerType.Layer1, new Layer(LayerType.Layer1, "Scoop L1", ".scl1", t => ScoopL1Grammar.Instance.CompilationUnits.Parse(t).GetResult()) },
            //{ LayerType.Layer2, new Layer(LayerType.Layer2, "Scoop L2", ".scl2", () => new ScoopL1Grammar()) }
        };

        public static Layer GetLayer(LayerType layerType) => _layers[layerType];

        public static TranspileResult TranspileFile(LayerType layerType, string inputFileName, string outputFileName = null)
        {
            CompilationUnitNode ast;
            var layer = GetLayer(layerType);
            using (var source = new StreamCharacterSequence(inputFileName, Encoding.UTF8))
            {
                var tokenizer = new Tokenizer(new TokenScanner(source));
                ast = layer.ParseFile(tokenizer);
                ast.FileName = inputFileName;
            }
            var validateResults = ast.Validate();
            if (validateResults.Any())
                return TranspileResult.ForValidationFailure(validateResults);

            outputFileName = outputFileName ?? inputFileName + ".cs";
            using (var outStream = new StreamWriter(outputFileName, false))
            {
                var preamble = Formatting.GetPreamble(layer.Name, inputFileName);
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
