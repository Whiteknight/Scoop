using System.Collections.Generic;
using NUnit.Framework;
using Scoop.SyntaxTree;
using Scoop.Tests.Utility;

namespace Scoop.Tests.Parsing
{
    [TestFixture]
    public class ParseCompilationUnitTests
    {
        [Test]
        public void ParseUnit_UsingDirective()
        {
            var target = new Parser();
            var result = target.ParseUnit("using A.B.C;");
            result.Should().MatchAst(
                new CompilationUnitNode
                {
                    UsingDirectives = new List<UsingDirectiveNode>
                    {
                        new UsingDirectiveNode
                        {
                            Namespace = new DottedIdentifierNode("A.B.C")
                        }
                    }
                }
            );
        }

        [Test]
        public void ParseUnit_Class()
        {
            var target = new Parser();
            var result = target.ParseUnit("namespace A { public class MyClass { } }");
            result.Should().MatchAst(
                new CompilationUnitNode
                {
                    Namespaces = new List<NamespaceNode>
                    {
                        new NamespaceNode
                        {
                            Name = new DottedIdentifierNode("A"),
                            Declarations = new List<AstNode>
                            {
                                new ClassNode
                                {
                                    AccessModifier = new KeywordNode("public"),
                                    Type = new KeywordNode("class"),
                                    Name = new IdentifierNode("MyClass"),
                                    Members = new List<AstNode>()
                                }
                            }
                        }
                    }
                }
            );
        }

        [Test]
        public void Namespace_Class_Attribute()
        {
            var target = new Parser();
            var result = target.ParseUnit(@"
namespace A 
{ 
    [MyAttr]
    public class MyClass 
    { 
    } 
}");
            result.Should().MatchAst(
                new CompilationUnitNode
                {
                    Namespaces = new List<NamespaceNode>
                    {
                        new NamespaceNode
                        {
                            Name = new DottedIdentifierNode("A"),
                            Declarations = new List<AstNode>
                            {
                                new ClassNode
                                {
                                    Attributes = new List<AttributeNode>
                                    {
                                        new AttributeNode
                                        {
                                            Type = new TypeNode("MyAttr")
                                        }
                                    },
                                    AccessModifier = new KeywordNode("public"),
                                    Type = new KeywordNode("class"),
                                    Name = new IdentifierNode("MyClass"),
                                    Members = new List<AstNode>()
                                }
                            }
                        }
                    }
                }
            );
        }

        

        [Test]
        public void ParseInterface()
        {
            var target = new Parser();
            var result = target.ParseUnit("namespace A { public interface MyInterface { } }");
            result.Should().MatchAst(
                new CompilationUnitNode
                {
                    Namespaces = new List<NamespaceNode>
                    {
                        new NamespaceNode
                        {
                            Name = new DottedIdentifierNode("A"),
                            Declarations = new List<AstNode>
                            {
                                new InterfaceNode
                                {
                                    AccessModifier = new KeywordNode("public"),
                                    Name = new IdentifierNode("MyInterface"),
                                    Members = new List<AstNode>()
                                }
                            }
                        }
                    }
                }
            );
        }
    }
}
