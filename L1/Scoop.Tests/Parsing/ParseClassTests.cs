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
                    Type = new KeywordNode("class"),
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
                    Type = new KeywordNode("class"),
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
                            ReturnType = new TypeNode
                            {
                                Name = new IdentifierNode("void")
                            },
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
                    Type = new KeywordNode("class"),
                    Name = new IdentifierNode("MyClass"),
                    Members = new List<AstNode>
                    {
                        new MethodNode
                        {
                            Name = new IdentifierNode("MyMethod"),
                            AccessModifier = new KeywordNode("public"),
                            ReturnType = new TypeNode
                            {
                                Name = new IdentifierNode("int")
                            },
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
                    Type = new KeywordNode("class"),
                    Name = new IdentifierNode("MyClass"),
                    Members = new List<AstNode>
                    {
                        new MethodNode
                        {
                            Name = new IdentifierNode("MyMethod"),
                            AccessModifier = new KeywordNode("public"),
                            ReturnType = new TypeNode
                            {
                                Name = new IdentifierNode("int")
                            },
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
                    Type = new KeywordNode("class"),
                    Name = new IdentifierNode("MyClass"),
                    Members = new List<AstNode>
                    {
                        new MethodNode
                        {
                            Name = new IdentifierNode("MyMethod"),
                            AccessModifier = new KeywordNode("public"),
                            ReturnType = new TypeNode
                            {
                                Name = new IdentifierNode("int")
                            },
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
                    Type = new KeywordNode("class"),
                    Name = new IdentifierNode("MyClass"),
                    Members = new List<AstNode>
                    {
                        new MethodNode
                        {
                            Name = new IdentifierNode("MyMethod"),
                            AccessModifier = new KeywordNode("public"),
                            ReturnType = new TypeNode
                            {
                                Name = new IdentifierNode("int")
                            },
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
                    Type = new KeywordNode("class"),
                    Name = new IdentifierNode("MyClass"),
                    Members = new List<AstNode>
                    {
                        new MethodNode
                        {
                            Name = new IdentifierNode("MyMethod"),
                            AccessModifier = new KeywordNode("public"),
                            ReturnType = new TypeNode
                            {
                                Name = new IdentifierNode("int")
                            },
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
                    Type = new KeywordNode("class"),
                    Name = new IdentifierNode("MyClass"),
                    Members = new List<AstNode>
                    {
                        new MethodNode
                        {
                            Name = new IdentifierNode("MyMethod"),
                            AccessModifier = new KeywordNode("public"),
                            ReturnType = new TypeNode
                            {
                                Name = new IdentifierNode("int")
                            },
                            Parameters = new List<AstNode>(),
                            Statements = new List<AstNode>
                            {
                                new ReturnNode
                                {
                                    Expression = new MemberAccessNode
                                    {
                                        Instance = new StringNode("\"test\""),
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
                    Type = new KeywordNode("class"),
                    Name = new IdentifierNode("MyClass"),
                    Members = new List<AstNode>
                    {
                        new MethodNode
                        {
                            Name = new IdentifierNode("MyMethod"),
                            AccessModifier = new KeywordNode("public"),
                            ReturnType = new TypeNode
                            {
                                Name = new IdentifierNode("string")
                            },
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

        [Test]
        public void ParseClass_GenericClassAndMethod()
        {
            var target = new Parser();
            var result = target.ParseClass(@"
public class MyClass<TA> 
{
    public TB MyMethod<TB, TC>() 
    { 
        return default(TB);
    }
}");
            result.Should().MatchAst(
                new ClassNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Type = new KeywordNode("class"),
                    Name = new IdentifierNode("MyClass"),
                    GenericTypeParameters = new List<AstNode>
                    {
                        new TypeNode("TA")
                    },
                    Members = new List<AstNode>
                    {
                        new MethodNode
                        {
                            AccessModifier = new KeywordNode("public"),
                            ReturnType = new TypeNode("TB"),
                            Name = new IdentifierNode("MyMethod"),
                            GenericTypeParameters = new List<AstNode>
                            {
                                new TypeNode("TB"),
                                new TypeNode("TC")
                            },
                            Parameters = new List<AstNode>(),
                            Statements = new List<AstNode>
                            {
                                new ReturnNode
                                {
                                    Expression = new InvokeNode
                                    {
                                        Instance = new IdentifierNode("default"),
                                        Arguments = new List<AstNode>
                                        {
                                            new IdentifierNode("TB")
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
        public void ParseClass_GenericClassAndMethodConstraints()
        {
            var target = new Parser();
            var result = target.ParseClass(@"
public class MyClass<TA> 
    where TA : class, new()
{
    public TB MyMethod<TB, TC>() 
        where TB : IMyInterface, new()
        where TC : class, IMyInterface
    { 
        return new TB();
    }
}");
            result.Should().MatchAst(
                new ClassNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Type = new KeywordNode("class"),
                    Name = new IdentifierNode("MyClass"),
                    GenericTypeParameters = new List<AstNode>
                    {
                        new TypeNode("TA")
                    },
                    TypeConstraints = new List<TypeConstraintNode>
                    {
                        new TypeConstraintNode
                        {
                            Type = new IdentifierNode("TA"),
                            Constraints = new List<AstNode>
                            {
                                new KeywordNode("class"),
                                new KeywordNode("new()")
                            }
                        }
                    },
                    Members = new List<AstNode>
                    {
                        new MethodNode
                        {
                            AccessModifier = new KeywordNode("public"),
                            ReturnType = new TypeNode("TB"),
                            Name = new IdentifierNode("MyMethod"),
                            GenericTypeParameters = new List<AstNode>
                            {
                                new TypeNode("TB"),
                                new TypeNode("TC")
                            },
                            Parameters = new List<AstNode>(),
                            TypeConstraints = new List<TypeConstraintNode>
                            {
                                new TypeConstraintNode
                                {
                                    Type = new IdentifierNode("TB"),
                                    Constraints = new List<AstNode>
                                    {
                                        new TypeNode("IMyInterface"),
                                        new KeywordNode("new()")
                                    }
                                },
                                new TypeConstraintNode
                                {
                                    Type = new IdentifierNode("TC"),
                                    Constraints = new List<AstNode>
                                    {
                                        new KeywordNode("class"),
                                        new TypeNode("IMyInterface")
                                    }
                                }
                            },
                            Statements = new List<AstNode>
                            {
                                new ReturnNode
                                {
                                    Expression = new NewNode
                                    {
                                        Type = new TypeNode("TB"),
                                        Arguments = new List<AstNode>()
                                    }
                                }
                            }
                        }
                    }
                }
            );
        }

        [Test]
        public void ParseClass_NestedClass()
        {
            var target = new Parser();
            var result = target.ParseClass(@"
public class MyClass 
{ 
    public class ChildClass { }
}");
            result.Should().MatchAst(
                new ClassNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Type = new KeywordNode("class"),
                    Name = new IdentifierNode("MyClass"),
                    Members = new List<AstNode>
                    {
                        new ClassNode
                        {
                            AccessModifier = new KeywordNode("public"),
                            Type = new KeywordNode("class"),
                            Name = new IdentifierNode("ChildClass"),
                            Members = new List<AstNode>()
                        }
                    }
                }
            );
        }

        [Test]
        public void ParseClass_NestedInterface()
        {
            var target = new Parser();
            var result = target.ParseClass(@"
public class MyClass 
{ 
    public interface ChildIFace { }
}");
            result.Should().MatchAst(
                new ClassNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Type = new KeywordNode("class"),
                    Name = new IdentifierNode("MyClass"),
                    Members = new List<AstNode>
                    {
                        new InterfaceNode
                        {
                            AccessModifier = new KeywordNode("public"),
                            Name = new IdentifierNode("ChildIFace"),
                            Members = new List<AstNode>()
                        }
                    }
                }
            );
        }

        [Test]
        public void ParseClass_InheritInterfaces()
        {
            var target = new Parser();
            var result = target.ParseClass("public class MyClass : IFaceA, IFaceB { }");
            result.Should().MatchAst(
                new ClassNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Type = new KeywordNode("class"),
                    Name = new IdentifierNode("MyClass"),
                    Interfaces = new List<AstNode>
                    {
                        new TypeNode
                        {
                            Name = new IdentifierNode("IFaceA")
                        },
                        new TypeNode
                        {
                            Name = new IdentifierNode("IFaceB")
                        }
                    },
                    Members = new List<AstNode>()
                }
            );
        }

        [Test]
        public void ParseClass_Struct()
        {
            var target = new Parser();
            var result = target.ParseClass("public struct MyClass { }");
            result.Should().MatchAst(
                new ClassNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Type = new KeywordNode("struct"),
                    Name = new IdentifierNode("MyClass"),
                    Members = new List<AstNode>()
                }
            );
        }

        [Test]
        public void ParseClass_Partial()
        {
            var target = new Parser();
            var result = target.ParseClass("public partial class MyClass { }");
            result.Should().MatchAst(
                new ClassNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Modifiers = new List<KeywordNode> { new KeywordNode("partial") },
                    Type = new KeywordNode("class"),
                    Name = new IdentifierNode("MyClass"),
                    Members = new List<AstNode>()
                }
            );
        }

        [Test]
        public void ParseClass_Const()
        {
            var target = new Parser();
            var result = target.ParseClass(@"
public class MyClass 
{
    private const string test = ""value"";
}");
            result.Should().MatchAst(
                new ClassNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Type = new KeywordNode("class"),
                    Name = new IdentifierNode("MyClass"),
                    Members = new List<AstNode>
                    {
                        new ConstNode
                        {
                            AccessModifier = new KeywordNode("private"),
                            Type = new TypeNode("string"),
                            Name = new IdentifierNode("test"),
                            Value = new StringNode("\"value\"")
                        }
                    }
                }
            );
        }

        [Test]
        public void ParseClass_Field()
        {
            var target = new Parser();
            var result = target.ParseClass(@"
public class MyClass 
{
    string _test;
}");
            result.Should().MatchAst(
                new ClassNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Type = new KeywordNode("class"),
                    Name = new IdentifierNode("MyClass"),
                    Members = new List<AstNode>
                    {
                        new FieldNode
                        {
                            Type = new TypeNode("string"),
                            Name = new IdentifierNode("_test")
                        }
                    }
                }
            );
        }
    }
}
