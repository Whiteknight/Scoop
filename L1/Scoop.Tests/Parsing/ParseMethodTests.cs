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
                                    Name = new IdentifierNode("int"),
                                    GenericArguments = new List<AstNode>()
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
                                                Name = new IdentifierNode("int"),
                                                GenericArguments = new List<AstNode>()
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
    }
}
