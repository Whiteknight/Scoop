using NUnit.Framework;
using Scoop.Parsing;
using Scoop.Parsing.Tokenization;
using Scoop.SyntaxTree;
using Scoop.Tests.Utility;
using static Scoop.Parsing.Parsers.ParserMethods;
using static Scoop.Parsing.Parsers.TokenParserMethods;

namespace Scoop.Tests.Parsers
{

    [TestFixture]
    public class ReplaceParserVisitorTests
    {
        [Test]
        public void Replace_Sequence()
        {
            var parser = Rule(
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
            Assert.Inconclusive("Can't test this right now because of strong typing problems");
            // Replace the "new(..)" parser with an Operator("."), then parse an expression with dots as terminals
            var parser = new ScoopGrammar().Expressions.FindNamed("Expressions") as IParser<Token, AstNode>;


            // ERROR: We can't replace IPArser<Token,NewNode> with IParser<Token,OperatorNode> because of strong typing in the tree
            // We could make this work if we jam a ReplaceableParser into the tree somewhere
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
