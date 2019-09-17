using NUnit.Framework;
using Scoop.SyntaxTree;
using Scoop.Tests.Utility;

namespace Scoop.Tests.Parsing
{
    [TestFixture]
    public class ParseStatementTests
    {
        [Test]
        public void DeclareVariable_ExplicitComplexType()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.Statements.Parse("x.y<z>[] myVar;");
            result.Should().MatchAst(
                new VariableDeclareNode
                {
                    Name = new IdentifierNode("myVar"),
                    Type = new TypeNode
                    {
                        Name = new IdentifierNode("x"),
                        Child = new TypeNode
                        {
                            Name = new IdentifierNode("y"),
                            GenericArguments = new ListNode<TypeNode>
                            {
                                Separator = new OperatorNode(","),
                                [0] = new TypeNode("z")
                            }
                        },
                        ArrayTypes = new ListNode<ArrayTypeNode> { new ArrayTypeNode() }
                    }
                }
            );
        }

        [Test]
        public void Return_Expression()
        {
            const string syntax = @"
return 1;";
            var ast = TestSuite.GetScoopGrammar().Statements.Parse(syntax);
            ast.Should().MatchAst(
                new ReturnNode
                {
                    Expression = new IntegerNode(1)
                }
            );
        }

        [Test]
        public void Return_NoExpression()
        {
            const string syntax = @"
return;";
            var ast = TestSuite.GetScoopGrammar().Statements.Parse(syntax);
            ast.Should().MatchAst(
                new ReturnNode()
            );
        }
    }
}
