using System;
using Scoop.Parsing;
using Scoop.Parsing.Parsers.Visitors;

namespace Scoop.GrammarGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var grammar = new ScoopGrammar();
            var target = new ScoopBnfStringifyVisitor();
            var s = target.ToBnf(grammar.CompilationUnits);
            Console.WriteLine(s);
            Console.ReadKey();
        }
    }
}
