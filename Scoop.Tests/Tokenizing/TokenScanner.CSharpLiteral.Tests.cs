using FluentAssertions;
using NUnit.Framework;
using Scoop.Tests.Utility;
using Scoop.Tokenization;

namespace Scoop.Tests.Tokenizing
{
    [TestFixture]
    public class TokenScanner_CSharpLiteral_Tests
    {

        [Test]
        public void ParseNext_CSharpLiteral_Simple()
        {
            var source = new StringCharacterSequence(@"c# {obj.Method();}");
            new TokenParser().Parse(source).Value.Should().Match(TokenType.CSharpLiteral, "obj.Method();");
        }

        [Test]
        public void ParseNext_CSharpLiteral_Braces()
        {
            var source = new StringCharacterSequence(@"c# {if(x==2){y();}}");
            new TokenParser().Parse(source).Value.Should().Match(TokenType.CSharpLiteral, "if(x==2){y();}");
        }

        [Test]
        public void ParseNext_CSharpLiteral_CharSimple()
        {
            var source = new StringCharacterSequence(@"c# {'a'}");

            new TokenParser().Parse(source).Value.Should().Match(TokenType.CSharpLiteral, "'a'");
        }

        [Test]
        public void ParseNext_CSharpLiteral_CharEscape()
        {
            var source = new StringCharacterSequence(@"c# {'\n'}");

            new TokenParser().Parse(source).Value.Should().Match(TokenType.CSharpLiteral, "'\\n'");
        }

        [Test]
        public void ParseNext_CSharpLiteral_StringSimple()
        {
            var source = new StringCharacterSequence(@"c# {""quoted""}");

            new TokenParser().Parse(source).Value.Should().Match(TokenType.CSharpLiteral, "\"quoted\"");
        }

        [Test]
        public void ParseNext_CSharpLiteral_StringEscapedQuote()
        {
            var source = new StringCharacterSequence(@"c# {""quo\""ted""}");

            new TokenParser().Parse(source).Value.Should().Match(TokenType.CSharpLiteral, "\"quo\\\"ted\"");
        }

        [Test]
        public void ParseNext_CSharpLiteral_StringBrace()
        {
            var source = new StringCharacterSequence(@"c# {""quo}ted""}");

            new TokenParser().Parse(source).Value.Should().Match(TokenType.CSharpLiteral, "\"quo}ted\"");
        }

        [Test]
        public void ParseNext_CSharpLiteral_AtStringSimple()
        {
            var source = new StringCharacterSequence(@"c# {@""quoted""}");

            new TokenParser().Parse(source).Value.Should().Match(TokenType.CSharpLiteral, "@\"quoted\"");
        }

        [Test]
        public void ParseNext_CSharpLiteral_AtStringEscapedQuote()
        {
            var source = new StringCharacterSequence(@"c# {@""quo""""ted""}");

            new TokenParser().Parse(source).Value.Should().Match(TokenType.CSharpLiteral, "@\"quo\"\"ted\"");
        }

        [Test]
        public void ParseNext_CSharpLiteral_AtStringBackslashEscapedQuote()
        {
            var source = new StringCharacterSequence(@"c# {@""quo\""""ted""}");

            new TokenParser().Parse(source).Value.Should().Match(TokenType.CSharpLiteral, "@\"quo\\\"\"ted\"");
        }

        [Test]
        public void ParseNext_CSharpLiteral_Character()
        {
            var source = new StringCharacterSequence(@"c# {'x'}");

            new TokenParser().Parse(source).Value.Should().Match(TokenType.CSharpLiteral, "'x'");
        }

        [Test]
        public void ParseNext_CSharpLiteral_CharacterEscapedQuote()
        {
            var source = new StringCharacterSequence(@"c# {'\''}");

            new TokenParser().Parse(source).Value.Should().Match(TokenType.CSharpLiteral, "'\\''");
        }

        [Test]
        public void ParseNext_CSharpLiteral_CharacterEscapedHexSequence()
        {
            var source = new StringCharacterSequence(@"c# {'\x1234'}");

            new TokenParser().Parse(source).Value.Should().Match(TokenType.CSharpLiteral, "'\\x1234'");
        }
    }
}