using FluentAssertions;
using NUnit.Framework;
using Scoop.Parsers;
using Scoop.SyntaxTree;

namespace Scoop.Tests.L1.Validation
{
    [TestFixture]
    public class TypesErrorsTests
    {
        [Test]
        public void Types_EmptyGenericTypeList()
        {
            const string syntax = @"a<>";
            var ast = TestSuite.GetScoopGrammar().Types.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingType);
        }

        [Test]
        public void Types_MissingCloseAngle()
        {
            const string syntax = @"a<b";
            var ast = TestSuite.GetScoopGrammar().Types.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingCloseAngle);
        }

        [Test]
        public void Types_MisingCloseBrace()
        {
            const string syntax = @"a[";
            var ast = TestSuite.GetScoopGrammar().Types.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingCloseBrace);
        }

        [Test]
        public void GenericTypeArguments_MissingCloseAngle()
        {
            const string syntax = @"public int x<b(){}";
            var ast = TestSuite.GetScoopGrammar().ClassMembers.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingCloseAngle);
        }
    }
}
