using System;
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
            target.ScanNext().Type.Should().Be(TokenType.EndOfInput);
        }

        [Test]
        public void ParseNext_Whitespace()
        {
            var target = new TokenScanner("   ");
            target.ScanNext().Should().Match(TokenType.Whitespace, "   ");
        }

        [Test]
        public void ParseNext_WhitespaceOperator()
        {
            var target = new TokenScanner("  + ");
            target.ScanNext().Should().Match(TokenType.Whitespace, "  ");
            target.ScanNext().Should().Match(TokenType.Operator, "+");
            target.ScanNext().Should().Match(TokenType.Whitespace, " ");
        }

        [Test]
        public void ParseNext_SingleLineComment()
        {
            var target = new TokenScanner(@"
                // This is a comment
");
            target.ScanNext().Type.Should().Be(TokenType.Whitespace);
            target.ScanNext().Should().Match(TokenType.Comment, " This is a comment");
            target.ScanNext().Type.Should().Be(TokenType.Whitespace);
        }

        [Test]
        public void ParseNext_MultiLineComment1()
        {
            var target = new TokenScanner(@"
                /* This is a comment */
");
            target.ScanNext().Type.Should().Be(TokenType.Whitespace);
            target.ScanNext().Should().Match(TokenType.Comment, " This is a comment ");
            target.ScanNext().Type.Should().Be(TokenType.Whitespace);
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
            target.ScanNext().Type.Should().Be(TokenType.Whitespace);
            target.ScanNext().Should().Match(TokenType.Comment, @" This
is
a
comment ");
            target.ScanNext().Type.Should().Be(TokenType.Whitespace);
        }

        [Test]
        public void ParseNext_IntegerDivideInteger()
        {
            var target = new TokenScanner(@"1/2");
            target.ScanNext().Should().Match(TokenType.Integer, "1");
            target.ScanNext().Should().Match(TokenType.Operator, "/");
            target.ScanNext().Should().Match(TokenType.Integer, "2");
        }

        [Test]
        public void ParseNext_ClassDefinition()
        {
            var target = new TokenScanner(@"public class MyClass { }");
            target.ScanNext().Should().Match(TokenType.Word, "public");
            target.ScanNext().Should().Match(TokenType.Whitespace, " ");
            target.ScanNext().Should().Match(TokenType.Word, "class");
            target.ScanNext().Should().Match(TokenType.Whitespace, " ");
            target.ScanNext().Should().Match(TokenType.Word, "MyClass");
            target.ScanNext().Should().Match(TokenType.Whitespace, " ");
            target.ScanNext().Should().Match(TokenType.Operator, "{");
            target.ScanNext().Should().Match(TokenType.Whitespace, " ");
            target.ScanNext().Should().Match(TokenType.Operator, "}");
        }

        [Test]
        public void ParseNext_MethodDefinition()
        {
            var target = new TokenScanner(@"public void MyMethod() { }");
            target.ScanNext().Should().Match(TokenType.Word, "public");
            target.ScanNext().Should().Match(TokenType.Whitespace, " ");
            target.ScanNext().Should().Match(TokenType.Word, "void");
            target.ScanNext().Should().Match(TokenType.Whitespace, " ");
            target.ScanNext().Should().Match(TokenType.Word, "MyMethod");
            target.ScanNext().Should().Match(TokenType.Operator, "(");
            target.ScanNext().Should().Match(TokenType.Operator, ")");
            target.ScanNext().Should().Match(TokenType.Whitespace, " ");
            target.ScanNext().Should().Match(TokenType.Operator, "{");
            target.ScanNext().Should().Match(TokenType.Whitespace, " ");
            target.ScanNext().Should().Match(TokenType.Operator, "}");
        }

        [Test]
        public void ParseNext_Character()
        {
            var target = new TokenScanner("'x'");
            target.ScanNext().Should().Match(TokenType.Character, "'x'");
        }

        [Test]
        public void ParseNext_CharacterEscapedQuote()
        {
            var target = new TokenScanner("'\\''");
            target.ScanNext().Should().Match(TokenType.Character, "'\\''");
        }

        [Test]
        public void ParseNext_CharacterEscapedHexValue()
        {
            var target = new TokenScanner("'\\x41'");
            target.ScanNext().Should().Match(TokenType.Character, "'\\x41'");
        }

        [Test]
        public void ParseNext_UnknownBackslash()
        {
            Action act = () => new TokenScanner("\\").ScanNext();
            act.Should().Throw<TokenizingException>();
        }

        [Test]
        public void ParseNext_UnknownBacktick()
        {
            Action act = () => new TokenScanner("`").ScanNext();
            act.Should().Throw<TokenizingException>();
        }

        [Test]
        public void ParseNext_Word()
        {
            var target = new TokenScanner("test class @class");
            target.ScanNext().Should().Match(TokenType.Word, "test");
            target.ScanNext().Type.Should().Be(TokenType.Whitespace);
            target.ScanNext().Should().Match(TokenType.Word, "class");
            target.ScanNext().Type.Should().Be(TokenType.Whitespace);
            target.ScanNext().Should().Match(TokenType.Word, "@class");
        }
    }
}