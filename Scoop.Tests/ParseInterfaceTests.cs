using System.Collections.Generic;
using NUnit.Framework;
using Scoop.SyntaxTree;
using Scoop.Tests.Utility;

namespace Scoop.Tests
{
    [TestFixture]
    public class ParseInterfaceTests
    {
        [Test]
        public void ParseInterface_Test()
        {
            var target = new Parser();
            var result = target.ParseInterface("public interface MyInterface { }");
            result.Should().MatchAst(
                new InterfaceNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Name = new IdentifierNode("MyInterface"),
                    Members = new List<AstNode>()
                }
            );
        }
    }
}