using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Sequences;
using Scoop.Parsing.Tokenization;
using Scoop.Tests.Utility;
using ParserObjects;

namespace Scoop.Tests.Tokenizing
{
    public class LexicalGrammarTests
    {
        [Test]
        public void ParseNext_EOF()
        {
            var target = LexicalGrammar.GetParser();
            var source = new StringCharacterSequence("");
            target.Parse(source).Value.Type.Should().Be(TokenType.EndOfInput);
        }

        [Test]
        public void ParseNext_WhitespaceOperator()
        {
            var target = LexicalGrammar.GetParser();
            var source = new StringCharacterSequence("  + ");
            target.Parse(source).Value.Should().Match(TokenType.Operator, "+");
        }

        [Test]
        public void ParseNext_SingleLineComment()
        {
            //Assert.Inconclusive("Comments aren't returned from the lexer right now");
            var target = LexicalGrammar.GetParser();
            var source = new StringCharacterSequence(@"
                // This is a comment
");
            var result = target.Parse(source).Value;
            result.Type.Should().Be(TokenType.EndOfInput);
            result.Frontmatter[1].Should().Be("// This is a comment");
        }

        [Test]
        public void ParseNext_MultiLineComment1()
        {
            var target = LexicalGrammar.GetParser();
            var source = new StringCharacterSequence(@"
                /* This is a comment */
");
            var result = target.Parse(source).Value;
            result.Type.Should().Be(TokenType.EndOfInput);
            result.Frontmatter[1].Should().Be("/* This is a comment */");
        }

        [Test]
        public void ParseNext_MultiLineComment2()
        {
            var target = LexicalGrammar.GetParser();
            var source = new StringCharacterSequence(@"
                /* This
is
a
comment */
");
            var result = target.Parse(source).Value;
            result.Type.Should().Be(TokenType.EndOfInput);
            result.Frontmatter[1].Should().Be(@"/* This
is
a
comment */");
        }

        [Test]
        public void ParseNext_IntegerDivideInteger()
        {
            var target = LexicalGrammar.GetParser();
            var source = new StringCharacterSequence(@"1/2");
            target.Parse(source).Value.Should().Match(TokenType.Integer, "1");
            target.Parse(source).Value.Should().Match(TokenType.Operator, "/");
            target.Parse(source).Value.Should().Match(TokenType.Integer, "2");
        }

        [Test]
        public void ParseNext_ClassDefinition()
        {
            var target = LexicalGrammar.GetParser();
            var source = new StringCharacterSequence(@"public class MyClass { }");
            target.Parse(source).Value.Should().Match(TokenType.Word, "public");
            target.Parse(source).Value.Should().Match(TokenType.Word, "class");
            target.Parse(source).Value.Should().Match(TokenType.Word, "MyClass");
            target.Parse(source).Value.Should().Match(TokenType.Operator, "{");
            target.Parse(source).Value.Should().Match(TokenType.Operator, "}");
        }

        [Test]
        public void ParseNext_MethodDefinition()
        {
            var target = LexicalGrammar.GetParser();
            var source = new StringCharacterSequence(@"public void MyMethod() { }");
            target.Parse(source).Value.Should().Match(TokenType.Word, "public");
            target.Parse(source).Value.Should().Match(TokenType.Word, "void");
            target.Parse(source).Value.Should().Match(TokenType.Word, "MyMethod");
            target.Parse(source).Value.Should().Match(TokenType.Operator, "(");
            target.Parse(source).Value.Should().Match(TokenType.Operator, ")");
            target.Parse(source).Value.Should().Match(TokenType.Operator, "{");
            target.Parse(source).Value.Should().Match(TokenType.Operator, "}");
        }

        [Test]
        public void ParseNext_Character()
        {
            var target = LexicalGrammar.GetParser();
            var source = new StringCharacterSequence("'x'");
            target.Parse(source).Value.Should().Match(TokenType.Character, "'x'");
        }

        [Test]
        public void ParseNext_CharacterEscapedQuote()
        {
            var target = LexicalGrammar.GetParser();
            var source = new StringCharacterSequence("'\\''");
            target.Parse(source).Value.Should().Match(TokenType.Character, "'\\''");
        }

        [Test]
        public void ParseNext_CharacterEscapedHexValue()
        {
            var target = LexicalGrammar.GetParser();
            var source = new StringCharacterSequence("'\\x41'");
            target.Parse(source).Value.Should().Match(TokenType.Character, "'\\x41'");
        }

        [Test]
        public void ParseNext_UnknownBackslash()
        {
            var target = LexicalGrammar.GetParser();
            var source = new StringCharacterSequence("\\");
            target.Parse(source).Value.Should().Match(TokenType.Unknown, "\\");
        }

        [Test]
        public void ParseNext_UnknownBacktick()
        {
            var target = LexicalGrammar.GetParser();
            var source = new StringCharacterSequence("`");
            target.Parse(source).Value.Should().Match(TokenType.Unknown, "`");
        }

        [Test]
        public void ParseNext_Word()
        {
            var target = LexicalGrammar.GetParser();
            var source = new StringCharacterSequence("test class @class");
            target.Parse(source).Value.Should().Match(TokenType.Word, "test");
            target.Parse(source).Value.Should().Match(TokenType.Word, "class");
            target.Parse(source).Value.Should().Match(TokenType.Word, "@class");
        }
    }
}
