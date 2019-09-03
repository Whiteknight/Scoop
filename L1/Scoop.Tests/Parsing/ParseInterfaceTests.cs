using System.Collections.Generic;
using NUnit.Framework;
using Scoop.SyntaxTree;
using Scoop.Tests.Utility;

namespace Scoop.Tests.Parsing
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

        [Test]
        public void ParseInterface_MethodDeclares()
        {
            var target = new Parser();
            var result = target.ParseInterface(@"
public interface MyInterface 
{ 
    int Method1();
    string Method2();
}");
            result.Should().MatchAst(
                new InterfaceNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Name = new IdentifierNode("MyInterface"),
                    Members = new List<AstNode>
                    {
                        new MethodDeclareNode
                        {
                            Name = new IdentifierNode("Method1"),
                            Parameters = new List<AstNode>(),
                            ReturnType = new TypeNode
                            {
                                Name = new IdentifierNode("int"),
                                GenericArguments = new List<AstNode>()
                            }
                        },
                        new MethodDeclareNode
                        {
                            Name = new IdentifierNode("Method2"),
                            ReturnType = new TypeNode
                            {
                                Name = new IdentifierNode("string"),
                                GenericArguments = new List<AstNode>()
                            },
                            Parameters = new List<AstNode>()
                        }
                    }
                }
            );
        }
    }
}