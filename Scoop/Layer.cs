using System;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop
{
    public class Layer
    {
        private readonly Func<ITokenizer, CompilationUnitNode> _parseFile;

        public Layer(LayerType type, string name, string extension, Func<ITokenizer, CompilationUnitNode> parseFile)
        {
            _parseFile = parseFile;
            Type = type;
            Name = name;
            FileExtension = extension;
        }

        public LayerType Type { get; set; }
        public string Name { get; set; }
        public string FileExtension { get; set; }

        public CompilationUnitNode ParseFile(ITokenizer tokenizer)
        {
            return _parseFile(tokenizer);
        }
    }
}