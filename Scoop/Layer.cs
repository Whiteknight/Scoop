using System;
using System.Collections.Generic;
using Scoop.Grammar;

namespace Scoop
{
    public class Layer
    {
        private readonly Func<IScoopGrammar> _createGrammar;
        private IScoopGrammar _grammar;

        public Layer(LayerType type, string name, string extension, Func<IScoopGrammar> createGrammar)
        {
            Type = type;
            Name = name;
            FileExtension = extension;
            _createGrammar = createGrammar;
        }

        public LayerType Type { get; set; }
        public string Name { get; set; }
        public string FileExtension { get; set; }

        public IScoopGrammar GetGrammar()
        {
            if (_grammar != null)
                return _grammar;
            _grammar = _createGrammar();
            return _grammar;
        }
    }
}