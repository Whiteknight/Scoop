using FluentAssertions;
using NUnit.Framework;
using Scoop.SyntaxTree;

namespace Scoop.Tests.Validation
{
    [TestFixture]
    public class EnumsErrorsTests
    {
        [Test]
        public void Enum_MissingName()
        {
            const string syntax = @"
enum 
{
    MemberA,
    MemberB = 1
}";
            var ast = new Parser().Enums.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingIdentifier);
        }

        [Test]
        public void Enum_MissingOpenBracket()
        {
            const string syntax = @"
enum MyEnum

    MemberA,
    MemberB = 1
}";
            var ast = new Parser().Enums.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingOpenBracket);
        }

        [Test]
        public void Enum_MissingCloseBracket()
        {
            const string syntax = @"
enum MyEnum
{
    MemberA,
    MemberB = 1
";
            var ast = new Parser().Enums.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingCloseBracket);
        }
    }
}
