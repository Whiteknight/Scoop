using NUnit.Framework;
using Scoop.Parsers;
using Scoop.SyntaxTree;
using Scoop.Tests.Utility;

namespace Scoop.Tests.Parsing
{
    [TestFixture]
    public class ParseEnumTests
    {
        [Test]
        public void Enum_Empty()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.Enums.Parse("public enum MyEnum { }");
            result.Should().MatchAst(
                new EnumNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Name = new IdentifierNode("MyEnum"),
                    Members = ListNode<EnumMemberNode>.Default()
                }
            );
        }

        [Test]
        public void Enum_Values()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.Enums.Parse("public enum MyEnum { A = 0, B, C }");
            result.Should().MatchAst(
                new EnumNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Name = new IdentifierNode("MyEnum"),
                    Members = new ListNode<EnumMemberNode>
                    {
                        Separator = new OperatorNode(","),
                        [0] = new EnumMemberNode
                        {
                            Name = new IdentifierNode("A"),
                            Value = new IntegerNode(0)
                        },
                        [1] = new EnumMemberNode
                        {
                            Name = new IdentifierNode("B")
                        },
                        [2] = new EnumMemberNode
                        {
                            Name = new IdentifierNode("C")
                        }
                    }
                }
            );
        }
    }
}
