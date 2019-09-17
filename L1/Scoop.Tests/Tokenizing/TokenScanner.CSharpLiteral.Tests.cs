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
            var target = new TokenScanner(@"c# {obj.Method();}");
            target.ScanNext().Should().Match(TokenType.CSharpLiteral, "obj.Method();");
        }

        [Test]
        public void ParseNext_CSharpLiteral_Braces()
        {
            var target = new TokenScanner(@"c# {if(x==2){y();}}");
            target.ScanNext().Should().Match(TokenType.CSharpLiteral, "if(x==2){y();}");
        }

        [Test]
        public void ParseNext_CSharpLiteral_CharSimple()
        {
            var target = new TokenScanner(@"c# {'a'}");

            target.ScanNext().Should().Match(TokenType.CSharpLiteral, "'a'");
        }

        [Test]
        public void ParseNext_CSharpLiteral_CharEscape()
        {
            var target = new TokenScanner(@"c# {'\n'}");

            target.ScanNext().Should().Match(TokenType.CSharpLiteral, "'\\n'");
        }

        [Test]
        public void ParseNext_CSharpLiteral_StringSimple()
        {
            var target = new TokenScanner(@"c# {""quoted""}");

            target.ScanNext().Should().Match(TokenType.CSharpLiteral, "\"quoted\"");
        }

        [Test]
        public void ParseNext_CSharpLiteral_StringEscapedQuote()
        {
            var target = new TokenScanner(@"c# {""quo\""ted""}");

            target.ScanNext().Should().Match(TokenType.CSharpLiteral, "\"quo\\\"ted\"");
        }

        [Test]
        public void ParseNext_CSharpLiteral_StringBrace()
        {
            var target = new TokenScanner(@"c# {""quo}ted""}");

            target.ScanNext().Should().Match(TokenType.CSharpLiteral, "\"quo}ted\"");
        }

        [Test]
        public void ParseNext_CSharpLiteral_AtStringSimple()
        {
            var target = new TokenScanner(@"c# {@""quoted""}");

            target.ScanNext().Should().Match(TokenType.CSharpLiteral, "@\"quoted\"");
        }

        [Test]
        public void ParseNext_CSharpLiteral_AtStringEscapedQuote()
        {
            var target = new TokenScanner(@"c# {@""quo""""ted""}");

            target.ScanNext().Should().Match(TokenType.CSharpLiteral, "@\"quo\"\"ted\"");
        }

        [Test]
        public void ParseNext_CSharpLiteral_AtStringBackslashEscapedQuote()
        {
            var target = new TokenScanner(@"c# {@""quo\""""ted""}");

            target.ScanNext().Should().Match(TokenType.CSharpLiteral, "@\"quo\\\"\"ted\"");
        }

        [Test]
        public void ParseNext_CSharpLiteral_Character()
        {
            var target = new TokenScanner(@"c# {'x'}");

            target.ScanNext().Should().Match(TokenType.CSharpLiteral, "'x'");
        }

        [Test]
        public void ParseNext_CSharpLiteral_CharacterEscapedQuote()
        {
            var target = new TokenScanner(@"c# {'\''}");

            target.ScanNext().Should().Match(TokenType.CSharpLiteral, "'\\''");
        }

        [Test]
        public void ParseNext_CSharpLiteral_CharacterEscapedHexSequence()
        {
            var target = new TokenScanner(@"c# {'\x1234'}");

            target.ScanNext().Should().Match(TokenType.CSharpLiteral, "'\\x1234'");
        }
    }
}