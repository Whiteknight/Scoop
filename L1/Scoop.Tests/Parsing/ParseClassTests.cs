using NUnit.Framework;
using Scoop.Parsers;
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
            var target = TestSuite.GetScoopGrammar();
            var result = target.Classes.Parse("public class MyClass { }");
            result.Should().MatchAst(
                new ClassNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Type = new KeywordNode("class"),
                    Name = new IdentifierNode("MyClass"),
                    Members = new ListNode<AstNode>()
                }
            );
        }

        [Test]
        public void ParseClass_CtorAndMethod()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.Classes.Parse(@"
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
                    Members = new ListNode<AstNode>
                    {
                        new ConstructorNode
                        {
                            ClassName = new IdentifierNode("MyClass"),
                            AccessModifier = new KeywordNode("public"),
                            Parameters = ListNode<ParameterNode>.Default(),
                            Statements = new ListNode<AstNode>()
                        },
                        new MethodNode
                        {
                            Name = new IdentifierNode("MyMethod"),
                            AccessModifier = new KeywordNode("public"),
                            ReturnType = new TypeNode
                            {
                                Name = new IdentifierNode("void")
                            },
                            Parameters = ListNode<ParameterNode>.Default(),
                            Statements = new ListNode<AstNode>()
                        }
                    }
                }
            );
        }

        [Test]
        public void ParseClass_CtorThis()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.Classes.Parse(@"
public class MyClass 
{
    public MyClass() : this(1) { }

    public MyClass(int a) : this(a, ""test"") { }

    public MyClass(int a, string b) { }
}");
            result.Should().MatchAst(
                new ClassNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Type = new KeywordNode("class"),
                    Name = new IdentifierNode("MyClass"),
                    Members = new ListNode<AstNode>
                    {
                        new ConstructorNode
                        {
                            ClassName = new IdentifierNode("MyClass"),
                            AccessModifier = new KeywordNode("public"),
                            Parameters = ListNode<ParameterNode>.Default(),
                            ThisArgs = new ListNode<AstNode>
                            {
                                Separator = new OperatorNode(","),
                                [0] = new IntegerNode(1)
                            },
                            Statements = new ListNode<AstNode>()
                        },
                        new ConstructorNode
                        {
                            ClassName = new IdentifierNode("MyClass"),
                            AccessModifier = new KeywordNode("public"),
                            Parameters = new ListNode<ParameterNode>
                            {
                                Separator = new OperatorNode(","),
                                [0] = new ParameterNode
                                {
                                    Type = new TypeNode("int"),
                                    Name = new IdentifierNode("a")
                                }
                            },
                            ThisArgs = new ListNode<AstNode>
                            {
                                Separator = new OperatorNode(","),
                                [0] = new IdentifierNode("a"),
                                [1] = new StringNode("\"test\"")
                            },
                            Statements = new ListNode<AstNode>()
                        },
                        new ConstructorNode
                        {
                            ClassName = new IdentifierNode("MyClass"),
                            AccessModifier = new KeywordNode("public"),
                            Parameters = new ListNode<ParameterNode>
                            {
                                Separator = new OperatorNode(","),
                                [0] = new ParameterNode
                                {
                                    Type = new TypeNode("int"),
                                    Name = new IdentifierNode("a")
                                },
                                [1] = new ParameterNode
                                {
                                    Type = new TypeNode("string"),
                                    Name = new IdentifierNode("b")
                                }
                            },
                            Statements = new ListNode<AstNode>()
                        }
                    }
                }
            );
        }

        [Test]
        public void ParseClass_MethodReturnNumber()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.Classes.Parse(@"
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
                    Members = new ListNode<AstNode>
                    {
                        new MethodNode
                        {
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
        public void ParseClass_LambdaMethodReturnNumber()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.Classes.Parse(@"
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
                    Members = new ListNode<AstNode>
                    {
                        new MethodNode
                        {
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
        public void ParseClass_MethodReturnInfixExpression()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.Classes.Parse(@"
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
                    Members = new ListNode<AstNode>
                    {
                        new MethodNode
                        {
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
            var target = TestSuite.GetScoopGrammar();
            var result = target.Classes.Parse(@"
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
                    Members = new ListNode<AstNode>
                    {
                        new MethodNode
                        {
                            Name = new IdentifierNode("MyMethod"),
                            AccessModifier = new KeywordNode("public"),
                            ReturnType = new TypeNode
                            {
                                Name = new IdentifierNode("int")
                            },
                            Parameters = ListNode<ParameterNode>.Default(),
                            Statements = new ListNode<AstNode>
                            {
                                new VariableDeclareNode
                                {
                                    Type = new TypeNode("var"),
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
            var target = TestSuite.GetScoopGrammar();
            var result = target.Classes.Parse(@"
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
                    Members = new ListNode<AstNode>
                    {
                        new MethodNode
                        {
                            Name = new IdentifierNode("MyMethod"),
                            AccessModifier = new KeywordNode("public"),
                            ReturnType = new TypeNode
                            {
                                Name = new IdentifierNode("int")
                            },
                            Parameters = ListNode<ParameterNode>.Default(),
                            Statements = new ListNode<AstNode>
                            {
                                new VariableDeclareNode
                                {
                                    Type = new TypeNode("var"),
                                    Name = new IdentifierNode("value"),
                                    Value = new IntegerNode(4)
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
            var target = TestSuite.GetScoopGrammar();
            var result = target.Classes.Parse(@"
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
                    Members = new ListNode<AstNode>
                    {
                        new MethodNode
                        {
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
                                    Expression = new MemberAccessNode
                                    {
                                        Instance = new StringNode("\"test\""),
                                        MemberName = new IdentifierNode("Length"),
                                        Operator = new OperatorNode(".")
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
            var target = TestSuite.GetScoopGrammar();
            var result = target.Classes.Parse(@"
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
                    Members = new ListNode<AstNode>
                    {
                        new MethodNode
                        {
                            Name = new IdentifierNode("MyMethod"),
                            AccessModifier = new KeywordNode("public"),
                            ReturnType = new TypeNode
                            {
                                Name = new IdentifierNode("string")
                            },
                            Parameters = ListNode<ParameterNode>.Default(),
                            Statements = new ListNode<AstNode>
                            {
                                new ReturnNode
                                {
                                    Expression = new InvokeNode
                                    {
                                        Instance = new MemberAccessNode
                                        {
                                            Instance = new IntegerNode(5),
                                            MemberName = new IdentifierNode("ToString"),
                                            Operator = new OperatorNode(".")
                                        },
                                        Arguments = ListNode<AstNode>.Default()
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
            var target = TestSuite.GetScoopGrammar();
            var result = target.Classes.Parse(@"
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
                    GenericTypeParameters = new ListNode<IdentifierNode>
                    {
                        Separator = new OperatorNode(","),
                        [0] = new IdentifierNode("TA")
                    },
                    Members = new ListNode<AstNode>
                    {
                        new MethodNode
                        {
                            AccessModifier = new KeywordNode("public"),
                            ReturnType = new TypeNode("TB"),
                            Name = new IdentifierNode("MyMethod"),
                            GenericTypeParameters = new ListNode<IdentifierNode>
                            {
                                Separator = new OperatorNode(","),
                                [0] = new IdentifierNode("TB"),
                                [1] = new IdentifierNode("TC")
                            },
                            Parameters = ListNode<ParameterNode>.Default(),
                            Statements = new ListNode<AstNode>
                            {
                                new ReturnNode
                                {
                                    Expression = new InvokeNode
                                    {
                                        Instance = new IdentifierNode("default"),
                                        Arguments = new ListNode<AstNode>
                                        {
                                            Separator = new OperatorNode(","),
                                            [0] = new IdentifierNode("TB")
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
            var target = TestSuite.GetScoopGrammar();
            var result = target.Classes.Parse(@"
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
                    GenericTypeParameters = new ListNode<IdentifierNode>
                    {
                        Separator = new OperatorNode(","),
                        [0] = new IdentifierNode("TA")
                    },
                    TypeConstraints = new ListNode<TypeConstraintNode>
                    {
                        new TypeConstraintNode
                        {
                            Type = new IdentifierNode("TA"),
                            Constraints = new ListNode<AstNode>
                            {
                                Separator = new OperatorNode(","),
                                [0] = new KeywordNode("class"),
                                [1] = new KeywordNode("new()")
                            }
                        }
                    },
                    Members = new ListNode<AstNode>
                    {
                        new MethodNode
                        {
                            AccessModifier = new KeywordNode("public"),
                            ReturnType = new TypeNode("TB"),
                            Name = new IdentifierNode("MyMethod"),
                            GenericTypeParameters = new ListNode<IdentifierNode>
                            {
                                Separator = new OperatorNode(","),
                                [0] = new IdentifierNode("TB"),
                                [1] = new IdentifierNode("TC")
                            },
                            Parameters = ListNode<ParameterNode>.Default(),
                            TypeConstraints = new ListNode<TypeConstraintNode>
                            {
                                new TypeConstraintNode
                                {
                                    Type = new IdentifierNode("TB"),
                                    Constraints = new ListNode<AstNode>
                                    {
                                        Separator = new OperatorNode(","),
                                        [0] = new TypeNode("IMyInterface"),
                                        [1] = new KeywordNode("new()")
                                    }
                                },
                                new TypeConstraintNode
                                {
                                    Type = new IdentifierNode("TC"),
                                    Constraints = new ListNode<AstNode>
                                    {
                                        Separator = new OperatorNode(","),
                                        [0] = new KeywordNode("class"),
                                        [1] = new TypeNode("IMyInterface")
                                    }
                                }
                            },
                            Statements = new ListNode<AstNode>
                            {
                                new ReturnNode
                                {
                                    Expression = new NewNode
                                    {
                                        Type = new TypeNode("TB"),
                                        Arguments = ListNode<AstNode>.Default()
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
            var target = TestSuite.GetScoopGrammar();
            var result = target.Classes.Parse(@"
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
                    Members = new ListNode<AstNode>
                    {
                        new ClassNode
                        {
                            AccessModifier = new KeywordNode("public"),
                            Type = new KeywordNode("class"),
                            Name = new IdentifierNode("ChildClass"),
                            Members = new ListNode<AstNode>()
                        }
                    }
                }
            );
        }

        [Test]
        public void ParseClass_NestedInterface()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.Classes.Parse(@"
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
                    Members = new ListNode<AstNode>
                    {
                        new InterfaceNode
                        {
                            AccessModifier = new KeywordNode("public"),
                            Name = new IdentifierNode("ChildIFace"),
                            Members = new ListNode<MethodDeclareNode>()
                        }
                    }
                }
            );
        }

        [Test]
        public void ParseClass_InheritInterfaces()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.Classes.Parse("public class MyClass : IFaceA, IFaceB { }");
            result.Should().MatchAst(
                new ClassNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Type = new KeywordNode("class"),
                    Name = new IdentifierNode("MyClass"),
                    Interfaces = new ListNode<TypeNode>
                    {
                        Separator = new OperatorNode(","),
                        [0] = new TypeNode
                        {
                            Name = new IdentifierNode("IFaceA")
                        },
                        [1] = new TypeNode
                        {
                            Name = new IdentifierNode("IFaceB")
                        }
                    },
                    Members = new ListNode<AstNode>()
                }
            );
        }

        [Test]
        public void ParseClass_Struct()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.Classes.Parse("public struct MyClass { }");
            result.Should().MatchAst(
                new ClassNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Type = new KeywordNode("struct"),
                    Name = new IdentifierNode("MyClass"),
                    Members = new ListNode<AstNode>()
                }
            );
        }

        [Test]
        public void ParseClass_Partial()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.Classes.Parse("public partial class MyClass { }");
            result.Should().MatchAst(
                new ClassNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Modifiers = new ListNode<KeywordNode> { new KeywordNode("partial") },
                    Type = new KeywordNode("class"),
                    Name = new IdentifierNode("MyClass"),
                    Members = new ListNode<AstNode>()
                }
            );
        }

        [Test]
        public void ParseClass_Const()
        {
            var target = TestSuite.GetScoopGrammar();
            var result = target.Classes.Parse(@"
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
                    Members = new ListNode<AstNode>
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
            var target = TestSuite.GetScoopGrammar();
            var result = target.Classes.Parse(@"
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
                    Members = new ListNode<AstNode>
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
