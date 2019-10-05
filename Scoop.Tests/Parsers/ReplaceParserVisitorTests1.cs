using NUnit.Framework;
using Scoop.Grammar;
using Scoop.Parsers;
using Scoop.SyntaxTree;
using Scoop.Tests.Utility;
using static Scoop.Parsers.ScoopParsers;

namespace Scoop.Tests.Parsers
{

    [TestFixture]
    public class ReplaceParserVisitorTests
    {
        [Test]
        public void Replace_Sequence()
        {
            var parser = Sequence(
                Operator("("),
                Operator("*").Named("Star"),
                Operator(")"),
                (a, c, b) => c
            );

            var find = parser.FindNamed("Star");
            var replacement = Operator("+");
            parser = parser.Replace(find, replacement);
            var result = parser.Parse("(+)");
            result.Should().MatchAst(
                new OperatorNode("+")
            );
        }

        [Test]
        public void Replace_First()
        {
            var parser = First(
                Operator("+"),
                Operator("-").Named("Minus"),
                Operator("*")
            );

            var find = parser.FindNamed("Minus");
            var replacement = Operator("/");
            parser = parser.Replace(find, replacement);
            var result = parser.Parse("/");
            result.Should().MatchAst(
                new OperatorNode("/")
            );
        }

        [Test]
        public void Replace_Expressions_New()
        {
            // Replace the "new(..)" parser with an Operator("."), then parse an expression with dots as terminals
            var parser = new ScoopGrammar().Expressions.FindNamed("Expressions") as IParser<AstNode>;

            var find = parser.FindNamed("new");
            var replacement = Operator(".");
            parser = parser.Replace(find, replacement);
            var result = parser.Parse(". + .");
            
            result.Should().MatchAst(
                new InfixOperationNode
                {
                    Left = new OperatorNode("."),
                    Operator = new OperatorNode("+"),
                    Right = new OperatorNode(".")
                }
            );
        }
    }
}
