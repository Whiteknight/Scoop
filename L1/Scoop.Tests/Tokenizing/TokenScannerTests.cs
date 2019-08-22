using FluentAssertions;
using NUnit.Framework;
using Scoop.Tests.Utility;
using Scoop.Tokenization;

namespace Scoop.Tests.Tokenizing
{
    public class TokenScannerTests
    {
        [Test]
        public void ParseNext_EOF()
        {
            var target = new TokenScanner("");
            target.ParseNext().Type.Should().Be(TokenType.EndOfInput);
        }

        [Test]
        public void ParseNext_Whitespace()
        {
            var target = new TokenScanner("   ");
            target.ParseNext().Should().Match(TokenType.Whitespace, "   ");
        }

        [Test]
        public void ParseNext_WhitespaceOperator()
        {
            var target = new TokenScanner("  + ");
            target.ParseNext().Should().Match(TokenType.Whitespace, "  ");
            target.ParseNext().Should().Match(TokenType.Operator, "+");
            target.ParseNext().Should().Match(TokenType.Whitespace, " ");
        }

        [Test]
        public void ParseNext_SingleLineComment()
        {
            var target = new TokenScanner(@"
                // This is a comment
");
            target.ParseNext().Type.Should().Be(TokenType.Whitespace);
            target.ParseNext().Should().Match(TokenType.Comment, " This is a comment");
            target.ParseNext().Type.Should().Be(TokenType.Whitespace);
        }

        [Test]
        public void ParseNext_MultiLineComment1()
        {
            var target = new TokenScanner(@"
                /* This is a comment */
");
            target.ParseNext().Type.Should().Be(TokenType.Whitespace);
            target.ParseNext().Should().Match(TokenType.Comment, " This is a comment ");
            target.ParseNext().Type.Should().Be(TokenType.Whitespace);
        }

        [Test]
        public void ParseNext_MultiLineComment2()
        {
            var target = new TokenScanner(@"
                /* This
is
a
comment */
");
            target.ParseNext().Type.Should().Be(TokenType.Whitespace);
            target.ParseNext().Should().Match(TokenType.Comment, @" This
is
a
comment ");
            target.ParseNext().Type.Should().Be(TokenType.Whitespace);
        }

        [Test]
        public void ParseNext_IntegerDivideInteger()
        {
            var target = new TokenScanner(@"1/2");
            target.ParseNext().Should().Match(TokenType.Integer, "1");
            target.ParseNext().Should().Match(TokenType.Operator, "/");
            target.ParseNext().Should().Match(TokenType.Integer, "2");
        }

        [Test]
        public void ParseNext_ClassDefinition()
        {
            var target = new TokenScanner(@"public class MyClass { }");
            target.ParseNext().Should().Match(TokenType.Keyword, "public");
            target.ParseNext().Should().Match(TokenType.Whitespace, " ");
            target.ParseNext().Should().Match(TokenType.Keyword, "class");
            target.ParseNext().Should().Match(TokenType.Whitespace, " ");
            target.ParseNext().Should().Match(TokenType.Identifier, "MyClass");
            target.ParseNext().Should().Match(TokenType.Whitespace, " ");
            target.ParseNext().Should().Match(TokenType.Operator, "{");
            target.ParseNext().Should().Match(TokenType.Whitespace, " ");
            target.ParseNext().Should().Match(TokenType.Operator, "}");
        }

        [Test]
        public void ParseNext_MethodDefinition()
        {
            var target = new TokenScanner(@"public void MyMethod() { }");
            target.ParseNext().Should().Match(TokenType.Keyword, "public");
            target.ParseNext().Should().Match(TokenType.Whitespace, " ");
            target.ParseNext().Should().Match(TokenType.Identifier, "void");
            target.ParseNext().Should().Match(TokenType.Whitespace, " ");
            target.ParseNext().Should().Match(TokenType.Identifier, "MyMethod");
            target.ParseNext().Should().Match(TokenType.Operator, "(");
            target.ParseNext().Should().Match(TokenType.Operator, ")");
            target.ParseNext().Should().Match(TokenType.Whitespace, " ");
            target.ParseNext().Should().Match(TokenType.Operator, "{");
            target.ParseNext().Should().Match(TokenType.Whitespace, " ");
            target.ParseNext().Should().Match(TokenType.Operator, "}");
        }
    }
}