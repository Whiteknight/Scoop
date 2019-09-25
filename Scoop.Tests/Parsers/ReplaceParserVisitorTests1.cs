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
                Keyword("class").Named("ClassKeyword"),
                Operator(")"),
                (a, c, b) => c
            );

            var find = parser.FindNamed("ClassKeyword");
            var replacement = Keyword("interface");
            parser = parser.Replace(find, replacement);
            var result = parser.Parse("(interface)");
            result.Should().MatchAst(
                new KeywordNode("interface")
            );
        }

        [Test]
        public void Replace_First()
        {
            var parser = First(
                Keyword("struct"),
                Keyword("class").Named("ClassKeyword"),
                Keyword("namespace")
            );

            var find = parser.FindNamed("ClassKeyword");
            var replacement = Keyword("interface");
            parser = parser.Replace(find, replacement);
            var result = parser.Parse("interface");
            result.Should().MatchAst(
                new KeywordNode("interface")
            );
        }

        [Test]
        public void Replace_Expressions_New()
        {
            // Replace the "new(..)" parser with an Operator("."), then parse an expression with dots as terminals
            var parser = new ScoopL1Grammar().Expressions.FindNamed("Expressions") as IParser<AstNode>;

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
