using NUnit.Framework;
using Scoop.Parsing;
using Scoop.SyntaxTree;
using Scoop.Tests.Utility;

namespace Scoop.Tests.L1.Parsing
{
    [TestFixture]
    public class ParseNumericValuesTests
    {
        [Test]
        public void Double_Test()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Expressions.Parse("123.45");
            result.Should().MatchAst(
                new DoubleNode(123.45)
            );
        }

        [Test]
        public void Float_Test()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Expressions.Parse("123.45F");
            result.Should().MatchAst(
                new FloatNode(123.45F)
            );
        }

        [Test]
        public void Decimal_Test()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Expressions.Parse("123.45M");
            result.Should().MatchAst(
                new DecimalNode(123.45M)
            );
        }

        [Test]
        public void Long_Test()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Expressions.Parse("123L");
            result.Should().MatchAst(
                new LongNode(123L)
            );
        }

        [Test]
        public void UInteger_Test()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Expressions.Parse("123U");
            result.Should().MatchAst(
                new UIntegerNode(123U)
            );
        }

        [Test]
        public void ULong_Test()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Expressions.Parse("123UL");
            result.Should().MatchAst(
                new ULongNode(123UL)
            );
        }
    }
}