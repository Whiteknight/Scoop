using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Scoop.Grammar;
using Scoop.Parsers;
using Scoop.SyntaxTree;
using Scoop.Tests.Utility;

namespace Scoop.Tests.L2.Transpiling
{
    [TestFixture]
    public class NamedConstructorsTests
    {
        [Test]
        public void New_Named()
        {
            var target = TestSuite.GetGrammar().WithNamedConstructors();
            var result = target.Statements.Parse(@"new MyClass:Test();");
            result.Should().MatchAst(
                new NewNode
                {
                    Type = new TypeNode
                    {
                        Name = new IdentifierNode("MyClass")
                    },
                    Name = new IdentifierNode("Test"),
                    Arguments = ListNode<AstNode>.Default()
                }
            );
        }

        [Test]
        public void Constructor_Named()
        {
            var target = TestSuite.GetGrammar().WithNamedConstructors();
            var result = target.Classes.Parse(@"
public class MyClass 
{
    public MyClass:Test() { }
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
                            AccessModifier = new KeywordNode("public"),
                            ClassName = new IdentifierNode("MyClass"),
                            Name = new IdentifierNode("Test"),
                            Parameters = ListNode<ParameterNode>.Default(),
                            Statements = new ListNode<AstNode>
                            {
                                Items = new List<AstNode>()
                            }
                        }
                    }
                }
            );
        }

        [Test]
        public void NamedConstructor_Test()
        {

            var ast = TestSuite.GetGrammar().WithNamedConstructors().CompilationUnits.Parse(@"
namespace NamedConstructorsTests
{
    public class ValueClass 
    {
        string _value;

        public ValueClass:A()
        {
            _value = ""A"";
        }

        public ValueClass:B()
        {
            _value = ""B"";
        }

        public ValueClass:C(string letter)
        {
            _value = ""C"" + letter;
        }

        public ValueClass:D(string letter)
        {
            _value = ""D"" + letter;
        }

        public string GetValue() => _value;
    }

    public class Caller
    {
        public string GetValues()
        {
            return 
                new ValueClass:A().GetValue() + 
                new ValueClass:B().GetValue() + 
                new ValueClass:C(""X"").GetValue() + 
                new ValueClass:D(""Y"").GetValue();
        }
    }
}
");

            var assembly = TestCompiler.Compile(ast);
            var type = assembly.ExportedTypes.First(t => t.Name == "Caller");
            var myObj = Activator.CreateInstance(type);
            var method = type.GetMethod("GetValues");
            var result = (string) method.Invoke(myObj, new object[0]);
            result.Should().Be("ABCXDY");
        }
    }
}
