using FluentAssertions;
using NUnit.Framework;
using Scoop.SyntaxTree;

namespace Scoop.Tests.Validation
{
    [TestFixture]
    public class TypesErrorsTests
    {
        [Test]
        public void Types_EmptyGenericTypeList()
        {
            const string syntax = @"a<>";
            var ast = new Parser().Types.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingType);
        }

        [Test]
        public void Types_MissingCloseAngle()
        {
            const string syntax = @"a<b";
            var ast = new Parser().Types.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingCloseAngle);
        }

        [Test]
        public void Types_MisingCloseBrace()
        {
            const string syntax = @"a[";
            var ast = new Parser().Types.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingCloseBrace);
        }

        [Test]
        public void GenericTypeArguments_MissingCloseAngle()
        {
            const string syntax = @"<b";
            var ast = new Parser().GenericTypeArguments.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingCloseAngle);
        }

        [Test]
        public void GenericTypeParameters_MissingCloseAngle()
        {
            const string syntax = @"<b";
            var ast = new Parser().GenericTypeParameters.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingCloseAngle);
        }

        [Test]
        public void TypeConstraints_MissingIdentifier()
        {
            const string syntax = @"where  : class";
            var ast = new Parser().TypeConstraints.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingIdentifier);
        }

        [Test]
        public void TypeConstraints_MissingColon()
        {
            const string syntax = @"where T  class";
            var ast = new Parser().TypeConstraints.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingColon);
        }

        [Test]
        public void TypeConstraints_NewMissingOpenParan()
        {
            const string syntax = @"where T : new)";
            var ast = new Parser().TypeConstraints.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingOpenParen);
        }

        [Test]
        public void TypeConstraints_NewMissingCloseParan()
        {
            const string syntax = @"where T : new(";
            var ast = new Parser().TypeConstraints.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingCloseParen);
        }
    }
}
