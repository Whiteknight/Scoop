using System.Linq;
using NUnit.Framework;
using Scoop.Parsers;
using Scoop.SyntaxTree;
using Scoop.Tests.Utility;

namespace Scoop.Tests.Parsing
{
    [TestFixture]
    public class ParseAttributesTests
    {
        [Test]
        public void Attribute_NoArgs()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.Attributes.Parse(@"[MyAttr]").First();
            result.Should().MatchAst(
                new AttributeNode
                {
                    Type = new TypeNode("MyAttr"),
                }
            );
        }

        [Test]
        public void Attribute_3NoArgs()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.Attributes.Parse(@"
[MyAttrA]
[MyAttrB,MyAttrC]
[return:MyAttrD]");
            result.Should().MatchAst(
                new ListNode<AttributeNode>
                {

                    new AttributeNode
                    {
                        Type = new TypeNode("MyAttrA"),
                    },
                    new AttributeNode
                    {
                        Type = new TypeNode("MyAttrB"),
                    },
                    new AttributeNode
                    {
                        Type = new TypeNode("MyAttrC"),
                    },
                    new AttributeNode
                    {
                        Type = new TypeNode("MyAttrD"),
                        Target = new KeywordNode("return")
                    }
                }
            );
        }

        [Test]
        public void Attribute_NoArgsTarget()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.Attributes.Parse(@"[return:MyAttr]").First();
            result.Should().MatchAst(
                new AttributeNode
                {
                    Type = new TypeNode("MyAttr"),
                    Target = new KeywordNode("return")
                }
            );
        }

        [Test]
        public void Attribute_NoArgsParens()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.Attributes.Parse(@"[MyAttr()]").First();
            result.Should().MatchAst(
                new AttributeNode
                {
                    Type = new TypeNode("MyAttr"),
                    Arguments = ListNode<AstNode>.Default()
                }
            );
        }

        [Test]
        public void Attribute_PositionalArgs()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.Attributes.Parse(@"[MyAttr(1, 2, 3)]").First();
            result.Should().MatchAst(
                new AttributeNode
                {
                    Type = new TypeNode("MyAttr"),
                    Arguments = new ListNode<AstNode>
                    {
                        Separator = new OperatorNode(","),
                        [0] = new IntegerNode(1),
                        [1] = new IntegerNode(2),
                        [2] = new IntegerNode(3)
                    }
                }
            );
        }

        [Test]
        public void Attribute_NamedArgs()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.Attributes.Parse(@"[MyAttr(test = 1)]").First();
            result.Should().MatchAst(
                new AttributeNode
                {
                    Type = new TypeNode("MyAttr"),
                    Arguments = new ListNode<AstNode>
                    {
                        Separator = new OperatorNode(","),
                        [0] = new NamedArgumentNode
                        {
                            Name = new IdentifierNode("test"),
                            Separator = new OperatorNode("="),
                            Value = new IntegerNode(1)
                        }
                    }
                }
            );
        }

        [Test]
        public void Attribute_OnClass()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.Classes.Parse(@"
[MyAttr]
public class MyClass 
{ 
} ");
            result.Should().MatchAst(
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
            );
        }

        [Test]
        public void Attribute_OnClassMethod()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.Classes.Parse(@"
public class MyClass 
{
    [MyAttrA]
    [return: MyAttrB]
    public int MyMethod() 
    { 
        return 5;
    }
}");
            result.Should().MatchAst(
                new ClassNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Type = new KeywordNode("class"),
                    Name = new IdentifierNode("MyClass"),
                    Members = new ListNode<AstNode>
                    {
                        new MethodNode
                        {
                            Attributes = new ListNode<AttributeNode>
                            {
                                new AttributeNode
                                {
                                    Type = new TypeNode("MyAttrA")
                                },
                                new AttributeNode
                                {
                                    Target = new KeywordNode("return"),
                                    Type = new TypeNode("MyAttrB")
                                }
                            },
                            Name = new IdentifierNode("MyMethod"),
                            AccessModifier = new KeywordNode("public"),
                            ReturnType = new TypeNode
                            {
                                Name = new IdentifierNode("int")
                            },
                            Parameters = ListNode<ParameterNode>.Default(),
                            Statements = new ListNode<AstNode>
                            {
                                new ReturnNode
                                {
                                    Expression = new IntegerNode(5)
                                }
                            }
                        }
                    }
                }
            );
        }

        [Test]
        public void Attribute_OnInterface()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.Interfaces.Parse(@"
[MyAttr]
public interface MyInterface 
{ 
} ");
            result.Should().MatchAst(
                new InterfaceNode
                {
                    Attributes = new ListNode<AttributeNode>
                    {
                        new AttributeNode
                        {
                            Type = new TypeNode("MyAttr")
                        }
                    },
                    AccessModifier = new KeywordNode("public"),
                    Name = new IdentifierNode("MyInterface"),
                    Members = new ListNode<MethodDeclareNode>()
                }
            );
        }

        [Test]
        public void Attribute_OnMethodParameter()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.ClassMembers.Parse(@"
public void MyMethod([MyAttr] int x)
{
}");
            result.Should().MatchAst(
                new MethodNode
                {
                    Name = new IdentifierNode("MyMethod"),
                    AccessModifier = new KeywordNode("public"),
                    ReturnType = new TypeNode("void"),
                    Parameters = new ListNode<ParameterNode>
                    {
                        Separator = new OperatorNode(","),
                        [0] = new ParameterNode
                        {
                            Attributes = new ListNode<AttributeNode>
                            {
                                new AttributeNode
                                {
                                    Type = new TypeNode("MyAttr")
                                }
                            },
                            Type = new TypeNode("int"),
                            Name = new IdentifierNode("x")
                        }
                    },
                    Statements = new ListNode<AstNode>()
                }
            );
        }

        [Test]
        public void Attribute_OnEnum()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.Enums.Parse("[MyAttr] public enum MyEnum { }");
            result.Should().MatchAst(
                new EnumNode
                {
                    Attributes = new ListNode<AttributeNode>
                    {
                        new AttributeNode
                        {
                            Type = new TypeNode("MyAttr")
                        }
                    },
                    AccessModifier = new KeywordNode("public"),
                    Name = new IdentifierNode("MyEnum"),
                    Members = ListNode<EnumMemberNode>.Default()
                }
            );
        }

        [Test]
        public void Attribute_OnEnumMember()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.Enums.Parse(@" 
public enum MyEnum 
{
    [MyAttr]
    Value1
}");
            result.Should().MatchAst(
                new EnumNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Name = new IdentifierNode("MyEnum"),
                    Members = new ListNode<EnumMemberNode>
                    {
                        Separator = new OperatorNode(","),
                        [0] = new EnumMemberNode
                        {
                            Attributes = new ListNode<AttributeNode>
                            {
                                new AttributeNode
                                {
                                    Type = new TypeNode("MyAttr")
                                }
                            },
                            Name = new IdentifierNode("Value1")
                        }
                    }
                }
            );
        }
    }
}
