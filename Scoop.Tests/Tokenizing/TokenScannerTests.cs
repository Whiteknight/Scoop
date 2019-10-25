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
            var target = new TokenParser();
            var source = new StringCharacterSequence("");
            target.Parse(source).Value.Type.Should().Be(TokenType.EndOfInput);
        }

        [Test]
        public void ParseNext_Whitespace()
        {
            var target = new TokenParser();
            var source = new StringCharacterSequence("   ");
            target.Parse(source).Value.Should().Match(TokenType.Whitespace, "   ");
        }

        [Test]
        public void ParseNext_WhitespaceOperator()
        {
            var target = new TokenParser();
            var source = new StringCharacterSequence("  + ");
            target.Parse(source).Value.Should().Match(TokenType.Whitespace, "  ");
            target.Parse(source).Value.Should().Match(TokenType.Operator, "+");
            target.Parse(source).Value.Should().Match(TokenType.Whitespace, " ");
        }

        [Test]
        public void ParseNext_SingleLineComment()
        {
            var target = new TokenParser();
            var source = new StringCharacterSequence(@"
                // This is a comment
");
            target.Parse(source).Value.Type.Should().Be(TokenType.Whitespace);
            target.Parse(source).Value.Should().Match(TokenType.Comment, " This is a comment");
            target.Parse(source).Value.Type.Should().Be(TokenType.Whitespace);
        }

        [Test]
        public void ParseNext_MultiLineComment1()
        {
            var target = new TokenParser();
            var source = new StringCharacterSequence(@"
                /* This is a comment */
");
            target.Parse(source).Value.Type.Should().Be(TokenType.Whitespace);
            target.Parse(source).Value.Should().Match(TokenType.Comment, " This is a comment ");
            target.Parse(source).Value.Type.Should().Be(TokenType.Whitespace);
        }

        [Test]
        public void ParseNext_MultiLineComment2()
        {
            var target = new TokenParser();
            var source = new StringCharacterSequence(@"
                /* This
is
a
comment */
");
            target.Parse(source).Value.Type.Should().Be(TokenType.Whitespace);
            target.Parse(source).Value.Should().Match(TokenType.Comment, @" This
is
a
comment ");
            target.Parse(source).Value.Type.Should().Be(TokenType.Whitespace);
        }

        [Test]
        public void ParseNext_IntegerDivideInteger()
        {
            var target = new TokenParser();
            var source = new StringCharacterSequence(@"1/2");
            target.Parse(source).Value.Should().Match(TokenType.Integer, "1");
            target.Parse(source).Value.Should().Match(TokenType.Operator, "/");
            target.Parse(source).Value.Should().Match(TokenType.Integer, "2");
        }

        [Test]
        public void ParseNext_ClassDefinition()
        {
            var target = new TokenParser();
            var source = new StringCharacterSequence(@"public class MyClass { }");
            target.Parse(source).Value.Should().Match(TokenType.Word, "public");
            target.Parse(source).Value.Should().Match(TokenType.Whitespace, " ");
            target.Parse(source).Value.Should().Match(TokenType.Word, "class");
            target.Parse(source).Value.Should().Match(TokenType.Whitespace, " ");
            target.Parse(source).Value.Should().Match(TokenType.Word, "MyClass");
            target.Parse(source).Value.Should().Match(TokenType.Whitespace, " ");
            target.Parse(source).Value.Should().Match(TokenType.Operator, "{");
            target.Parse(source).Value.Should().Match(TokenType.Whitespace, " ");
            target.Parse(source).Value.Should().Match(TokenType.Operator, "}");
        }

        [Test]
        public void ParseNext_MethodDefinition()
        {
            var target = new TokenParser();
            var source = new StringCharacterSequence(@"public void MyMethod() { }");
            target.Parse(source).Value.Should().Match(TokenType.Word, "public");
            target.Parse(source).Value.Should().Match(TokenType.Whitespace, " ");
            target.Parse(source).Value.Should().Match(TokenType.Word, "void");
            target.Parse(source).Value.Should().Match(TokenType.Whitespace, " ");
            target.Parse(source).Value.Should().Match(TokenType.Word, "MyMethod");
            target.Parse(source).Value.Should().Match(TokenType.Operator, "(");
            target.Parse(source).Value.Should().Match(TokenType.Operator, ")");
            target.Parse(source).Value.Should().Match(TokenType.Whitespace, " ");
            target.Parse(source).Value.Should().Match(TokenType.Operator, "{");
            target.Parse(source).Value.Should().Match(TokenType.Whitespace, " ");
            target.Parse(source).Value.Should().Match(TokenType.Operator, "}");
        }

        [Test]
        public void ParseNext_Character()
        {
            var target = new TokenParser();
            var source = new StringCharacterSequence("'x'");
            target.Parse(source).Value.Should().Match(TokenType.Character, "'x'");
        }

        [Test]
        public void ParseNext_CharacterEscapedQuote()
        {
            var target = new TokenParser();
            var source = new StringCharacterSequence("'\\''");
            target.Parse(source).Value.Should().Match(TokenType.Character, "'\\''");
        }

        [Test]
        public void ParseNext_CharacterEscapedHexValue()
        {
            var target = new TokenParser();
            var source = new StringCharacterSequence("'\\x41'");
            target.Parse(source).Value.Should().Match(TokenType.Character, "'\\x41'");
        }

        [Test]
        public void ParseNext_UnknownBackslash()
        {
            var target = new TokenParser();
            var source = new StringCharacterSequence("\\");
            Action act = () => target.Parse(source);
            act.Should().Throw<TokenizingException>();
        }

        [Test]
        public void ParseNext_UnknownBacktick()
        {
            var target = new TokenParser();
            var source = new StringCharacterSequence("`");
            Action act = () => target.Parse(source);
            act.Should().Throw<TokenizingException>();
        }

        [Test]
        public void ParseNext_Word()
        {
            var target = new TokenParser();
            var source = new StringCharacterSequence("test class @class");
            target.Parse(source).Value.Should().Match(TokenType.Word, "test");
            target.Parse(source).Value.Type.Should().Be(TokenType.Whitespace);
            target.Parse(source).Value.Should().Match(TokenType.Word, "class");
            target.Parse(source).Value.Type.Should().Be(TokenType.Whitespace);
            target.Parse(source).Value.Should().Match(TokenType.Word, "@class");
        }
    }
}