using NUnit.Framework;
using Scoop.Parsers;
using Scoop.SyntaxTree;
using Scoop.Tests.Utility;

namespace Scoop.Tests.L1.Parsing
{
    [TestFixture]
    public class ParseCompilationUnitTests
    {
        [Test]
        public void ParseUnit_UsingDirective()
        {
            var target = TestSuite.GetGrammar();
            var result = target.CompilationUnits.Parse("using A.B.C;");
            result.Should().MatchAst(
                new CompilationUnitNode
                {
                    Members = new ListNode<AstNode>
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
        public void ParseUnit_AssemblyAttributes()
        {
            var target = TestSuite.GetGrammar();
            var result = target.CompilationUnits.Parse("[assembly:MyAttr]");
            result.Should().MatchAst(
                new CompilationUnitNode
                {
                    Members = new ListNode<AstNode>
                    {
                        new ListNode<AttributeNode>
                        {
                            Separator = new OperatorNode(","),
                            [0] = new AttributeNode
                            {
                                Target = new KeywordNode("assembly"),
                                Type = new TypeNode("MyAttr")
                            }
                        }
                    }
                }
            );
        }

        [Test]
        public void ParseUnit_Class()
        {
            var target = TestSuite.GetGrammar();
            var result = target.CompilationUnits.Parse("namespace A { public class MyClass { } }");
            result.Should().MatchAst(
                new CompilationUnitNode
                {
                    Members = new ListNode<AstNode>
                    {
                        new NamespaceNode
                        {
                            Name = new DottedIdentifierNode("A"),
                            Declarations = new ListNode<AstNode>
                            {
                                new ClassNode
                                {
                                    AccessModifier = new KeywordNode("public"),
                                    Type = new KeywordNode("class"),
                                    Name = new IdentifierNode("MyClass"),
                                    Members = new ListNode<AstNode>()
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
            var target = TestSuite.GetGrammar();
            var result = target.CompilationUnits.Parse(@"
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
                    Members = new ListNode<AstNode>
                    {
                        new NamespaceNode
                        {
                            Name = new DottedIdentifierNode("A"),
                            Declarations = new ListNode<AstNode>
                            {
                                new ClassNode
                                {
                                    Attributes = new ListNode<AttributeNode>
                                    {
                                        new AttributeNode
                                        {
                                            Type = new TypeNode("MyAttr")
                                        }
                                    },
                                    AccessModifier = new KeywordNode("public"),
                                    Type = new KeywordNode("class"),
                                    Name = new IdentifierNode("MyClass"),
                                    Members = new ListNode<AstNode>()
                                }
                            }
                        }
                    }
                }
            );
        }

        

        [Test]
        public void CompilationUnit_Interface()
        {
            var target = TestSuite.GetGrammar();
            var result = target.CompilationUnits.Parse("namespace A { public interface MyInterface { } }");
            result.Should().MatchAst(
                new CompilationUnitNode
                {
                    Members = new ListNode<AstNode>
                    {
                        new NamespaceNode
                        {
                            Name = new DottedIdentifierNode("A"),
                            Declarations = new ListNode<AstNode>
                            {
                                new InterfaceNode
                                {
                                    AccessModifier = new KeywordNode("public"),
                                    Name = new IdentifierNode("MyInterface"),
                                    Members = new ListNode<MethodDeclareNode>()
                                }
                            }
                        }
                    }
                }
            );
        }
    }
}
