using NUnit.Framework;
using ParserObjects.Sequences;
using Scoop.Parsing.Tokenization;
using Scoop.Tests.Utility;

namespace Scoop.Tests.Tokenizing
{
    [TestFixture]
    public class LexicalGrammarCSharpLiteralTests
    {

        [Test]
        public void ParseNext_CSharpLiteral_Simple()
        {
            var source = new StringCharacterSequence(@"c# {obj.Method();}");
            LexicalGrammar.GetParser().Parse(source).Value.Should().Match(TokenType.CSharpLiteral, "obj.Method();");
        }

        [Test]
        public void ParseNext_CSharpLiteral_Braces()
        {
            var source = new StringCharacterSequence(@"c# {if(x==2){y();}}");
            LexicalGrammar.GetParser().Parse(source).Value.Should().Match(TokenType.CSharpLiteral, "if(x==2){y();}");
        }

        [Test]
        public void ParseNext_CSharpLiteral_CharSimple()
        {
            var source = new StringCharacterSequence(@"c# {'a'}");

            LexicalGrammar.GetParser().Parse(source).Value.Should().Match(TokenType.CSharpLiteral, "'a'");
        }

        [Test]
        public void ParseNext_CSharpLiteral_CharEscape()
        {
            var source = new StringCharacterSequence(@"c# {'\n'}");

            LexicalGrammar.GetParser().Parse(source).Value.Should().Match(TokenType.CSharpLiteral, "'\\n'");
        }

        [Test]
        public void ParseNext_CSharpLiteral_StringSimple()
        {
            var source = new StringCharacterSequence(@"c# {""quoted""}");

            LexicalGrammar.GetParser().Parse(source).Value.Should().Match(TokenType.CSharpLiteral, "\"quoted\"");
        }

        [Test]
        public void ParseNext_CSharpLiteral_StringEscapedQuote()
        {
            var source = new StringCharacterSequence(@"c# {""quo\""ted""}");

            LexicalGrammar.GetParser().Parse(source).Value.Should().Match(TokenType.CSharpLiteral, "\"quo\\\"ted\"");
        }

        [Test]
        public void ParseNext_CSharpLiteral_StringBrace()
        {
            var source = new StringCharacterSequence(@"c# {""quo}ted""}");

            LexicalGrammar.GetParser().Parse(source).Value.Should().Match(TokenType.CSharpLiteral, "\"quo}ted\"");
        }

        [Test]
        public void ParseNext_CSharpLiteral_AtStringSimple()
        {
            var source = new StringCharacterSequence(@"c# {@""quoted""}");

            LexicalGrammar.GetParser().Parse(source).Value.Should().Match(TokenType.CSharpLiteral, "@\"quoted\"");
        }

        [Test]
        public void ParseNext_CSharpLiteral_AtStringEscapedQuote()
        {
            var source = new StringCharacterSequence(@"c# {@""quo""""ted""}");

            LexicalGrammar.GetParser().Parse(source).Value.Should().Match(TokenType.CSharpLiteral, "@\"quo\"\"ted\"");
        }

        [Test]
        public void ParseNext_CSharpLiteral_AtStringBackslashEscapedQuote()
        {
            var source = new StringCharacterSequence(@"c# {@""quo\""""ted""}");

            LexicalGrammar.GetParser().Parse(source).Value.Should().Match(TokenType.CSharpLiteral, "@\"quo\\\"\"ted\"");
        }

        [Test]
        public void ParseNext_CSharpLiteral_Character()
        {
            var source = new StringCharacterSequence(@"c# {'x'}");

            LexicalGrammar.GetParser().Parse(source).Value.Should().Match(TokenType.CSharpLiteral, "'x'");
        }

        [Test]
        public void ParseNext_CSharpLiteral_CharacterEscapedQuote()
        {
            var source = new StringCharacterSequence(@"c# {'\''}");

            LexicalGrammar.GetParser().Parse(source).Value.Should().Match(TokenType.CSharpLiteral, "'\\''");
        }

        [Test]
        public void ParseNext_CSharpLiteral_CharacterEscapedHexSequence()
        {
            var source = new StringCharacterSequence(@"c# {'\x1234'}");

            LexicalGrammar.GetParser().Parse(source).Value.Should().Match(TokenType.CSharpLiteral, "'\\x1234'");
        }
    }
}