using FluentAssertions;
using NUnit.Framework;
using Scoop.Parsers;
using Scoop.SyntaxTree;

namespace Scoop.Tests.L1.Validation
{
    [TestFixture]
    public class TopLevelErrorsTests
    {
        [Test]
        public void UsingDirective_MissingNamespace()
        {
            const string syntax = @"
using;";
            var ast = TestSuite.GetGrammar().CompilationUnits.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingNamespaceName);
        }

        [Test]
        public void UsingDirective_MissingSemicolon()
        {
            const string syntax = @"
using A.B.C";
            var ast = TestSuite.GetGrammar().CompilationUnits.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingSemicolon);
        }

        [Test]
        public void Namespace_MissingBody()
        {
            const string syntax = @"
namespace A.B.C";
            var ast = TestSuite.GetGrammar().CompilationUnits.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingOpenBracket);
        }

        [Test]
        public void Namespace_MissingCloseBracket()
        {
            const string syntax = @"
namespace A.B.C {";
            var ast = TestSuite.GetGrammar().CompilationUnits.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingCloseBracket);
        }

        [Test]
        public void Namespace_UnexpectedMember()
        {
            const string syntax = @"
namespace A.B.C {
    TEST
}";
            var ast = TestSuite.GetGrammar().CompilationUnits.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingCloseBracket);
        }
    }
}
