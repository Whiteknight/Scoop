using NUnit.Framework;
using Scoop.Parsing;
using Scoop.SyntaxTree;
using Scoop.Tests.Utility;

namespace Scoop.Tests.L1.Parsing
{
    [TestFixture]
    public class ParseStringValuesTests
    {
        [Test]
        public void String_Test()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Expressions.Parse("\"test\"");
            result.Should().MatchAst(
                new StringNode("\"test\"")
            );
        }

        [Test]
        public void String_EscapedQuote()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Expressions.Parse("\"test\\\"\"");
            result.Should().MatchAst(
                new StringNode("\"test\\\"\"")
            );
        }

        [Test]
        public void BlockString_Test()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Expressions.Parse("@\"test\"");
            result.Should().MatchAst(
                new StringNode("@\"test\"")
            );
        }

        [Test]
        public void BlockString_Quote()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Expressions.Parse("@\"test\"\"\"");
            result.Should().MatchAst(
                new StringNode("@\"test\"\"\"")
            );
        }

        [Test]
        public void InterpolatedString_Test()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Expressions.Parse("$\"test\"");
            result.Should().MatchAst(
                new StringNode("$\"test\"")
            );
        }

        [Test]
        public void InterpolatedString_EscapedQuote()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Expressions.Parse("$\"test\\\"\"");
            result.Should().MatchAst(
                new StringNode("$\"test\\\"\"")
            );
        }

        [Test]
        public void InterpolatedString_Int()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Expressions.Parse("$\"{5}\"");
            result.Should().MatchAst(
                new StringNode("$\"{5}\"")
            );
        }

        [Test]
        public void InterpolatedString_NestedString()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Expressions.Parse("$\"{\"test\"}\"");
            result.Should().MatchAst(
                new StringNode("$\"{\"test\"}\"")
            );
        }

        [Test]
        public void InterpolatedString_NestedBlock()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Expressions.Parse("$\"{@\"test\"}\"");
            result.Should().MatchAst(
                new StringNode("$\"{@\"test\"}\"")
            );
        }

        [Test]
        public void InterpolatedString_NestedInterpBlock()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Expressions.Parse("$\"{$@\"test\"}\"");
            result.Should().MatchAst(
                new StringNode("$\"{$@\"test\"}\"")
            );
        }

        [Test]
        public void InterpolatedString_NestedInterp()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Expressions.Parse("$\"{$\"test\"}\"");
            result.Should().MatchAst(
                new StringNode("$\"{$\"test\"}\"")
            );
        }

        [Test]
        public void InterpolatedBlockString_Test()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Expressions.Parse("$@\"test\"");
            result.Should().MatchAst(
                new StringNode("$@\"test\"")
            );
        }

        [Test]
        public void InterpolatedBlockString_Quote()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Expressions.Parse("$@\"test\"\"\"");
            result.Should().MatchAst(
                new StringNode("$@\"test\"\"\"")
            );
        }

        [Test]
        public void InterpolatedBlockString_NestedInteger()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Expressions.Parse("$@\"{5}\"");
            result.Should().MatchAst(
                new StringNode("$@\"{5}\"")
            );
        }
    }
}