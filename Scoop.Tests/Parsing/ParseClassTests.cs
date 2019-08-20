using System.Collections.Generic;
using NUnit.Framework;
using Scoop.SyntaxTree;
using Scoop.Tests.Utility;
using Scoop.Transpiler;

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

        [Test]
        public void ParseClass_CtorAndMethod()
        {
            var target = new Parser();
            var result = target.ParseClass(@"
public class MyClass 
{
    // default parameterless constructor
    public MyClass() { }

    // Simple void() method
    public void MyMethod() { }
}");
            var cs = CSharpTranspileVisitor.ToString(result);
            result.Should().MatchAst(
                new ClassNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Name = new IdentifierNode("MyClass"),
                    Members = new List<AstNode>
                    {
                        new ConstructorNode
                        {
                            ClassName = new IdentifierNode("MyClass"),
                            AccessModifier = new KeywordNode("public"),
                            Parameters = new List<AstNode>(),
                            Statements = new List<AstNode>()
                        },
                        new MethodNode
                        {
                            Name = new IdentifierNode("MyMethod"),
                            AccessModifier = new KeywordNode("public"),
                            ReturnType = new IdentifierNode("void"),
                            Parameters = new List<AstNode>(),
                            Statements = new List<AstNode>()
                        }
                    }
                }
            );
        }
    }
}
