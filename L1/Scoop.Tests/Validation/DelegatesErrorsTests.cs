using FluentAssertions;
using NUnit.Framework;
using Scoop.Parsers;
using Scoop.SyntaxTree;

namespace Scoop.Tests.Validation
{
    [TestFixture]
    public class DelegatesErrorsTests
    {
        [Test]
        public void Delegate_MissingEverything()
        {
            const string syntax = @"
delegate ";
            var ast = TestSuite.GetScoopGrammar().Delegates.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(5);
            // Semicolon is an unused member, it's hard to order those with respect to the used members
            // so we have to deal with them being out of order here.
            result[0].ErrorMessage.Should().Be(Errors.MissingSemicolon);
            result[1].ErrorMessage.Should().Be(Errors.MissingType);
            result[2].ErrorMessage.Should().Be(Errors.MissingIdentifier);
            result[3].ErrorMessage.Should().Be(Errors.MissingOpenParen);
            result[4].ErrorMessage.Should().Be(Errors.MissingCloseParen);
        }

        [Test]
        public void Delegate_MissingIdentifier()
        {
            // Missing the return type and missing the identifier are syntactically the same thing
            const string syntax = @"
delegate int (string x, double y);";
            var ast = TestSuite.GetScoopGrammar().Delegates.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingIdentifier);
        }

        [Test]
        public void Delegate_MissingOpenParen()
        {
            const string syntax = @"
delegate int MyDelegate string x, double y);";
            var ast = TestSuite.GetScoopGrammar().Delegates.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingOpenParen);
        }

        [Test]
        public void Delegate_MissingCloseParen()
        {
            const string syntax = @"
delegate int MyDelegate(string x, double y;";
            var ast = TestSuite.GetScoopGrammar().Delegates.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingCloseParen);
        }

        [Test]
        public void Delegate_MissingSemicolon()
        {
            const string syntax = @"
delegate int MyDelegate(string x, double y)";
            var ast = TestSuite.GetScoopGrammar().Delegates.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingSemicolon);
        }
    }
}
