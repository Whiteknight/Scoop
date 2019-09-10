using NUnit.Framework;
using Scoop.SyntaxTree;
using Scoop.Tests.Utility;

namespace Scoop.Tests.Parsing
{
    [TestFixture]
    public class ParseDelegatesTests
    {
        [Test]
        public void Delegate_Namespace()
        {
            var target = new Parser();
            var result = target.ParseUnit("namespace A { public delegate int MyDelegate(string x); }");
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
                                new DelegateNode
                                {
                                    AccessModifier = new KeywordNode("public"),
                                    ReturnType = new TypeNode("int"),
                                    Name = new IdentifierNode("MyDelegate"),
                                    Parameters = new ListNode<ParameterNode>
                                    {
                                        Separator = new OperatorNode(","),
                                        [0] = new ParameterNode
                                        {
                                            Type = new TypeNode("string"),
                                            Name = new IdentifierNode("x")
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            );
        }

        [Test]
        public void Delegate_Class()
        {
            var target = new Parser();
            var result = target.ParseClass(@"
public class MyClass 
{
     public delegate int MyDelegate(string x);
}");
            result.Should().MatchAst(
                new ClassNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Type = new KeywordNode("class"),
                    Name = new IdentifierNode("MyClass"),
                    Members = new ListNode<AstNode>
                    {
                        new DelegateNode
                        {
                            AccessModifier = new KeywordNode("public"),
                            ReturnType = new TypeNode("int"),
                            Name = new IdentifierNode("MyDelegate"),
                            Parameters = new ListNode<ParameterNode>
                            {
                                Separator = new OperatorNode(","),
                                [0] = new ParameterNode
                                {
                                    Type = new TypeNode("string"),
                                    Name = new IdentifierNode("x")
                                }
                            }
                        }
                    }
                }
            );
        }
    }
}
