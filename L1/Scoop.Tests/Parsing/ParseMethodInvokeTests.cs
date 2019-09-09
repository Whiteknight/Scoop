using System.Collections.Generic;
using NUnit.Framework;
using Scoop.SyntaxTree;
using Scoop.Tests.Utility;

namespace Scoop.Tests.Parsing
{
    [TestFixture]
    public class ParseMethodInvokeTests
    {
        [Test]
        public void InvokeMethod_Args()
        {
            var target = new Parser();
            var result = target.ParseStatement(@"myObj.Method(1, 'b');");
            result.Should().MatchAst(
                new InvokeNode
                {
                    Instance = new MemberAccessNode
                    {
                        Instance = new IdentifierNode("myObj"),
                        MemberName = new IdentifierNode("Method")
                    },
                    Arguments = new List<AstNode>
                    {
                        new IntegerNode(1),
                        new CharNode('b')
                    }
                }
            );
        }

        [Test]
        public void InvokeMethod_NullInvoke()
        {
            var target = new Parser();
            var result = target.ParseStatement(@"myObj?.Method();");
            result.Should().MatchAst(
                new InvokeNode
                {
                    Instance = new MemberAccessNode
                    {
                        Instance = new IdentifierNode("myObj"),
                        MemberName = new IdentifierNode("Method"),
                        IgnoreNulls = true
                    },
                    Arguments = new List<AstNode>()
                }
            );
        }

        [Test]
        public void InvokeMethod_NamedArgs()
        {
            var target = new Parser();
            var result = target.ParseStatement(@"func(test: 1);");
            result.Should().MatchAst(
                new InvokeNode
                {
                    Instance = new IdentifierNode("func"),
                    Arguments = new List<AstNode>
                    {
                        new NamedArgumentNode
                        {
                            Name = new IdentifierNode("test"),
                            Value = new IntegerNode(1)
                        }
                    }
                }
            );
        }
    }
}