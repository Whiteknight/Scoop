using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Scoop.SyntaxTree;
using Scoop.Tests.Utility;

namespace Scoop.Tests.Parsing
{
    [TestFixture]
    public class ParseAttributesTests
    {
        [Test]
        public void Attribute_OnClass()
        {
            var target = new Parser();
            var result = target.ParseClass(@"
[MyAttr]
public class MyClass 
{ 
} ");
            result.Should().MatchAst(
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
            );
        }

        [Test]
        public void Attribute_OnClassMethod()
        {
            var target = new Parser();
            var result = target.ParseClass(@"
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
                    Members = new List<AstNode>
                    {
                        new MethodNode
                        {
                            Attributes = new List<AttributeNode>
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
                            Parameters = new List<ParameterNode>(),
                            Statements = new List<AstNode>
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
            var target = new Parser();
            var result = target.ParseInterface(@"
[MyAttr]
public interface MyInterface 
{ 
} ");
            result.Should().MatchAst(
                new InterfaceNode
                {
                    Attributes = new List<AttributeNode>
                    {
                        new AttributeNode
                        {
                            Type = new TypeNode("MyAttr")
                        }
                    },
                    AccessModifier = new KeywordNode("public"),
                    Name = new IdentifierNode("MyInterface"),
                    Members = new List<AstNode>()
                }
            );
        }

        [Test]
        public void Attribute_OnMethodParameter()
        {
            var target = new Parser();
            var result = target.ParseClassMember(@"
public void MyMethod([MyAttr] int x)
{
}");
            result.Should().MatchAst(
                new MethodNode
                {
                    Name = new IdentifierNode("MyMethod"),
                    AccessModifier = new KeywordNode("public"),
                    ReturnType = new TypeNode("void"),
                    Parameters = new List<ParameterNode>
                    {
                        new ParameterNode
                        {
                            Attributes = new List<AttributeNode>
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
                    Statements = new List<AstNode>()
                }
            );
        }

        [Test]
        public void Attribute_OnEnum()
        {
            var target = new Parser();
            var result = target.ParseEnum("[MyAttr] public enum MyEnum { }");
            result.Should().MatchAst(
                new EnumNode
                {
                    Attributes = new List<AttributeNode>
                    {
                        new AttributeNode
                        {
                            Type = new TypeNode("MyAttr")
                        }
                    },
                    AccessModifier = new KeywordNode("public"),
                    Name = new IdentifierNode("MyEnum"),
                    Members = new List<EnumMemberNode>()
                }
            );
        }

        [Test]
        public void Attribute_OnEnumMember()
        {
            var target = new Parser();
            var result = target.ParseEnum(@" 
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
                    Members = new List<EnumMemberNode>
                    {
                        new EnumMemberNode
                        {
                            Attributes = new List<AttributeNode>
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
