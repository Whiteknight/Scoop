using System.Collections.Generic;
using NUnit.Framework;
using Scoop.SyntaxTree;
using Scoop.Tests.Utility;

namespace Scoop.Tests.Parsing
{
    [TestFixture]
    public class ParseClassTests
    {
        [Test]
        public void ParseClass_Test()
        {
            var target = new Parser();
            var result = target.ParseClass("public class MyClass { }");
            result.Should().MatchAst(
                new ClassNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Name = new IdentifierNode("MyClass"),
                    Members = new List<AstNode>()
                }
            );
        }
    }
}
