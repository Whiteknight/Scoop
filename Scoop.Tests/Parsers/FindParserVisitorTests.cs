using FluentAssertions;
using NUnit.Framework;
using Scoop.Grammar;
using Scoop.Parsers;

namespace Scoop.Tests.Parsers
{
    [TestFixture]
    public class FindParserVisitorTests
    {
        [Test]
        public void Find_new()
        {
            var grammar = new ScoopGrammar();
            var newParser = grammar.CompilationUnits.FindNamed("new");
            newParser.Should().NotBeNull();
            newParser.Name.Should().Be("new");
        }
    }
}
