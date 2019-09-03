using System.Collections.Generic;
using NUnit.Framework;
using Scoop.SyntaxTree;
using Scoop.Tests.Utility;

namespace Scoop.Tests.Parsing
{
    [TestFixture]
    public class ParseMethodTests
    {
        [Test]
        public void ParseMethod_NewListOfMyClass()
        {
            var target = new Parser();
            var result = target.ParseConstructorOrMethod(@"
public List<int[]> GetListOfIntArrays()
{
    return new List<int[]>();
}");
            result.Should().MatchAst(
                new MethodNode
                {
                    Name = new IdentifierNode("GetListOfIntArrays"),
                    AccessModifier = new KeywordNode("public"),
                    ReturnType = new TypeNode
                    {
                        Name = new IdentifierNode("List"),
                        GenericArguments = new List<AstNode>
                        {
                            new ArrayTypeNode
                            {
                                ElementType = new TypeNode
                                {
                                    Name = new IdentifierNode("int")
                                }
                            }
                        }
                    },
                    Parameters = new List<AstNode>(),
                    Statements = new List<AstNode>
                    {
                        new ReturnNode
                        {
                            Expression = new NewNode
                            {
                                Type = new TypeNode
                                {
                                    Name = new IdentifierNode("List"),
                                    GenericArguments = new List<AstNode>
                                    {
                                        new ArrayTypeNode
                                        {
                                            ElementType = new TypeNode
                                            {
                                                Name = new IdentifierNode("int")
                                            }
                                        }
                                    }
                                },
                                Arguments = new List<AstNode>()
                            }
                        }
                    }
                }
            );
        }

        [Test]
        public void ParseMethod_Parameters()
        {
            var target = new Parser();
            var result = target.ParseConstructorOrMethod(@"
public void TestMethod(int a, double b, string c)
{
}");
            result.Should().MatchAst(
                new MethodNode
                {
                    Name = new IdentifierNode("TestMethod"),
                    AccessModifier = new KeywordNode("public"),
                    ReturnType = new TypeNode
                    {
                        Name = new IdentifierNode("void")
                    },
                    Parameters = new List<AstNode>
                    {
                        new ParameterNode
                        {
                            Type = new TypeNode
                            {
                                Name = new IdentifierNode("int")
                            },
                            Name = new IdentifierNode("a")
                        },
                        new ParameterNode
                        {
                            Type = new TypeNode
                            {
                                Name = new IdentifierNode("double")
                            },
                            Name = new IdentifierNode("b")
                        },
                        new ParameterNode
                        {
                            Type = new TypeNode
                            {
                                Name = new IdentifierNode("string")
                            },
                            Name = new IdentifierNode("c")
                        }
                    },
                    Statements = new List<AstNode>()
                }
            );
        }
    }
}
