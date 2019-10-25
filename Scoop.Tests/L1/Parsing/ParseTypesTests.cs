using NUnit.Framework;
using Scoop.Parsing;
using Scoop.SyntaxTree;
using Scoop.Tests.Utility;

namespace Scoop.Tests.L1.Parsing
{
    public class ParseTypesTests
    {
        [Test]
        public void ArrayType_3Dimensions()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Types.Parse("int[,,]");
            result.Should().MatchAst(
                new TypeNode
                {
                    Name = new IdentifierNode("int"),
                    ArrayTypes = new ListNode<ArrayTypeNode>
                    {
                        new ArrayTypeNode { Dimensions = 3 }
                    }
                }
            );
        }

        [Test]
        public void ArrayType_3Level()
        {
            var target = TestSuite.GetGrammar();
            var result = target.Types.Parse("int[][][]");
            result.Should().MatchAst(
                new TypeNode
                {
                    Name = new IdentifierNode("int"),
                    ArrayTypes = new ListNode<ArrayTypeNode>
                    {
                        new ArrayTypeNode { Dimensions = 1 },
                        new ArrayTypeNode { Dimensions = 1 },
                        new ArrayTypeNode { Dimensions = 1 }
                    }
                }
            );
        }
    }
}
