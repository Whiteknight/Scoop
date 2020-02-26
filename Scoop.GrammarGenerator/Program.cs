using System;
using System.Text;
using Scoop.Parsing;
using Scoop.Parsing.Parsers.Visitors;

namespace Scoop.GrammarGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var grammar = new ScoopGrammar();
            var result = new StringBuilder();
            var target = new ScoopBnfStringifyVisitor(result);
            target.Visit(grammar.CompilationUnits);
            var s = result.ToString();
            Console.WriteLine(s);
            Console.ReadKey();
        }
    }
}
