using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Scoop.SyntaxTree;
using Scoop.Tests.Utility;

namespace Scoop.Tests.Parsing
{
    [TestFixture]
    public class ParseEnumTests
    {
        [Test]
        public void Enum_Empty()
        {
            var target = new Parser();
            var result = target.ParseEnum("public enum MyEnum { }");
            result.Should().MatchAst(
                new EnumNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Name = new IdentifierNode("MyEnum"),
                    Members = new List<EnumMemberNode>()
                }
            );
        }

        [Test]
        public void Enum_Values()
        {
            var target = new Parser();
            var result = target.ParseEnum("public enum MyEnum { A = 0, B, C }");
            result.Should().MatchAst(
                new EnumNode
                {
                    AccessModifier = new KeywordNode("public"),
                    Name = new IdentifierNode("MyEnum"),
                    Members = new List<EnumMemberNode>
                    {
                        new EnumMemberNode
                        {
                            Name = new IdentifierNode("A"),
                            Value = new IntegerNode(0)
                        },
                        new EnumMemberNode
                        {
                            Name = new IdentifierNode("B")
                        },
                        new EnumMemberNode
                        {
                            Name = new IdentifierNode("C")
                        }
                    }
                }
            );
        }
    }


}
