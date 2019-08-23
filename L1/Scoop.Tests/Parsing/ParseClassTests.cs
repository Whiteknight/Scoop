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

        [Test]
        public void ParseClass_MethodReturnNumber()
        {
            var target = new Parser();
            var result = target.ParseClass(@"
public class MyClass 
{
    public int MyMethod() 
    { 
        return 5;
    }
}");
            result.Should().MatchAst(
                new ClassNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Name = new IdentifierNode("MyClass"),
                    Members = new List<AstNode>
                    {
                        new MethodNode
                        {
                            Name = new IdentifierNode("MyMethod"),
                            AccessModifier = new KeywordNode("public"),
                            ReturnType = new IdentifierNode("int"),
                            Parameters = new List<AstNode>(),
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
        public void ParseClass_LambdaMethodReturnNumber()
        {
            var target = new Parser();
            var result = target.ParseClass(@"
public class MyClass 
{
    public int MyMethod() => 5;
}");
            result.Should().MatchAst(
                new ClassNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Name = new IdentifierNode("MyClass"),
                    Members = new List<AstNode>
                    {
                        new MethodNode
                        {
                            Name = new IdentifierNode("MyMethod"),
                            AccessModifier = new KeywordNode("public"),
                            ReturnType = new IdentifierNode("int"),
                            Parameters = new List<AstNode>(),
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
        public void ParseClass_MethodReturnInfixExpression()
        {
            var target = new Parser();
            var result = target.ParseClass(@"
public class MyClass 
{
    public int MyMethod() 
    { 
        return 5 + 6;
    }
}");
            result.Should().MatchAst(
                new ClassNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Name = new IdentifierNode("MyClass"),
                    Members = new List<AstNode>
                    {
                        new MethodNode
                        {
                            Name = new IdentifierNode("MyMethod"),
                            AccessModifier = new KeywordNode("public"),
                            ReturnType = new IdentifierNode("int"),
                            Parameters = new List<AstNode>(),
                            Statements = new List<AstNode>
                            {
                                new ReturnNode
                                {
                                    Expression = new InfixOperationNode
                                    {
                                        Left = new IntegerNode(5),
                                        Operator = new OperatorNode("+"),
                                        Right = new IntegerNode(6)
                                    }
                                }
                            }
                        }
                    }
                }
            );
        }

        [Test]
        public void ParseClass_MethodDeclareThenAssignReturn()
        {
            var target = new Parser();
            var result = target.ParseClass(@"
public class MyClass 
{
    public int MyMethod() 
    { 
        var value;
        value = 4;
        return value;
    }
}");
            result.Should().MatchAst(
                new ClassNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Name = new IdentifierNode("MyClass"),
                    Members = new List<AstNode>
                    {
                        new MethodNode
                        {
                            Name = new IdentifierNode("MyMethod"),
                            AccessModifier = new KeywordNode("public"),
                            ReturnType = new IdentifierNode("int"),
                            Parameters = new List<AstNode>(),
                            Statements = new List<AstNode>
                            {
                                new VariableDeclareNode {
                                    Name = new IdentifierNode("value")
                                },
                                new InfixOperationNode {
                                    Left = new IdentifierNode("value"),
                                    Operator = new OperatorNode("="),
                                    Right = new IntegerNode(4)
                                },
                                new ReturnNode
                                {
                                    Expression = new IdentifierNode("value")
                                }
                            }
                        }
                    }
                }
            );
        }

        [Test]
        public void ParseClass_MethodDeclareAssignReturn()
        {
            var target = new Parser();
            var result = target.ParseClass(@"
public class MyClass 
{
    public int MyMethod() 
    { 
        var value = 4;
        return value;
    }
}");
            result.Should().MatchAst(
                new ClassNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Name = new IdentifierNode("MyClass"),
                    Members = new List<AstNode>
                    {
                        new MethodNode
                        {
                            Name = new IdentifierNode("MyMethod"),
                            AccessModifier = new KeywordNode("public"),
                            ReturnType = new IdentifierNode("int"),
                            Parameters = new List<AstNode>(),
                            Statements = new List<AstNode>
                            {
                                new InfixOperationNode {
                                    Left = new VariableDeclareNode {
                                        Name = new IdentifierNode("value")
                                    },
                                    Operator = new OperatorNode("="),
                                    Right = new IntegerNode(4)
                                },
                                new ReturnNode
                                {
                                    Expression = new IdentifierNode("value")
                                }
                            }
                        }
                    }
                }
            );
        }

        [Test]
        public void ParseClass_MethodStringLiteralProperty()
        {
            var target = new Parser();
            var result = target.ParseClass(@"
public class MyClass 
{
    public int MyMethod() 
    { 
        return ""test"".Length;
    }
}");
            result.Should().MatchAst(
                new ClassNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Name = new IdentifierNode("MyClass"),
                    Members = new List<AstNode>
                    {
                        new MethodNode
                        {
                            Name = new IdentifierNode("MyMethod"),
                            AccessModifier = new KeywordNode("public"),
                            ReturnType = new IdentifierNode("int"),
                            Parameters = new List<AstNode>(),
                            Statements = new List<AstNode>
                            {
                                new ReturnNode
                                {
                                    Expression = new MemberAccessNode
                                    {
                                        Instance = new StringNode("test"),
                                        MemberName = new IdentifierNode("Length")
                                    }
                                }
                            }
                        }
                    }
                }
            );
        }

        [Test]
        public void ParseClass_MethodIntegerMethod()
        {
            var target = new Parser();
            var result = target.ParseClass(@"
public class MyClass 
{
    public string MyMethod() 
    { 
        return 5.ToString();
    }
}");
            result.Should().MatchAst(
                new ClassNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Name = new IdentifierNode("MyClass"),
                    Members = new List<AstNode>
                    {
                        new MethodNode
                        {
                            Name = new IdentifierNode("MyMethod"),
                            AccessModifier = new KeywordNode("public"),
                            ReturnType = new IdentifierNode("string"),
                            Parameters = new List<AstNode>(),
                            Statements = new List<AstNode>
                            {
                                new ReturnNode
                                {
                                    Expression = new InvokeNode
                                    {
                                        Instance = new MemberAccessNode
                                        {
                                            Instance = new IntegerNode(5),
                                            MemberName = new IdentifierNode("ToString")
                                        },
                                        Arguments = new List<AstNode>()
                                    }
                                }
                            }
                        }
                    }
                }
            );
        }
    }
}
