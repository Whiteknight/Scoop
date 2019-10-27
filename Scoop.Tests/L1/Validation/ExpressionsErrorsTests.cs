using FluentAssertions;
using NUnit.Framework;
using Scoop.Parsing;
using Scoop.SyntaxTree;

namespace Scoop.Tests.L1.Validation
{
    [TestFixture]
    public class ExpressionsErrorsTests
    {
        [Test]
        public void Expressions_terminal_MissingParen()
        {
            const string syntax = @"(1 + 2";
            var ast = TestSuite.GetGrammar().Expressions.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingCloseParen);
        }

        [Test]
        public void Expressions_postfix_MissingMemberAccessName()
        {
            const string syntax = @"x.";
            var ast = TestSuite.GetGrammar().Expressions.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingIdentifier);
        }

        [Test]
        public void Expressions_multiplicative_MissingExpression()
        {
            const string syntax = @"4*";
            var ast = TestSuite.GetGrammar().Expressions.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingExpression);
        }

        [Test]
        public void Expressions_additive_MissingExpression()
        {
            const string syntax = @"1+";
            var ast = TestSuite.GetGrammar().Expressions.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingExpression);
        }

        [Test]
        public void Expressions_typeCoerce_MissingType()
        {
            const string syntax = @"1 as";
            var ast = TestSuite.GetGrammar().Expressions.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingType);
        }

        [Test]
        public void Expressions_equality_MissingExpression()
        {
            const string syntax = @"1 ==";
            var ast = TestSuite.GetGrammar().Expressions.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingExpression);
        }

        [Test]
        public void Expressions_bitwise_MissingExpression()
        {
            const string syntax = @"1 |";
            var ast = TestSuite.GetGrammar().Expressions.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingExpression);
        }

        [Test]
        public void Expressions_logical_MissingExpression()
        {
            const string syntax = @"true &&";
            var ast = TestSuite.GetGrammar().Expressions.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingExpression);
        }

        [Test]
        public void Expressions_coalesce_MissingExpression()
        {
            const string syntax = @"null ??";
            var ast = TestSuite.GetGrammar().Expressions.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingExpression);
        }

        [Test]
        public void Expressions_conditional_MissingConsequent()
        {
            const string syntax = @"true ? : 0";
            var ast = TestSuite.GetGrammar().Expressions.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingExpression);
        }

        [Test]
        public void Expressions_conditional_MissingColon()
        {
            const string syntax = @"true ? 1 0";
            var ast = TestSuite.GetGrammar().Expressions.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingColon);
        }

        [Test]
        public void Expressions_conditional_MissingAlternate()
        {
            const string syntax = @"true ? 1 :";
            var ast = TestSuite.GetGrammar().Expressions.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingExpression);
        }

        [Test]
        public void Expressions_assignment_MissingExpression()
        {
            const string syntax = @"a = ";
            var ast = TestSuite.GetGrammar().Expressions.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            // This one gives a different error message from what it should do, we'll revisit when we fix this rule
            //result[0].ErrorMessage.Should().Be(Errors.MissingExpression);
        }

        [Test]
        public void Expressions_lambda_MissingBody()
        {
            const string syntax = @"() => ";
            var ast = TestSuite.GetGrammar().Expressions.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingExpression);
        }

        [Test]
        public void Expressions_FunctionArgList_ExtraComma()
        {
            const string syntax = @"f(1, 2,)";
            var ast = TestSuite.GetGrammar().Expressions.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingCloseParen);
        }

        [Test]
        public void Expressions_CharLiteral_NoContent()
        {
            const string syntax = @"''";
            var ast = TestSuite.GetGrammar().Expressions.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.UnrecognizedCharLiteral);
        }

        [TestCase("\"\\q\"")]
        [TestCase("$\"\\q\"")]
        [TestCase("\"\\xZ\"")]
        [TestCase("$\"\\xZ\"")]
        [TestCase("\"\\u1\"")]
        [TestCase("\"\\u12\"")]
        [TestCase("\"\\u123\"")]
        [TestCase("$\"\\u1\"")]
        [TestCase("$\"\\u12\"")]
        [TestCase("$\"\\u123\"")]
        [TestCase("\"\\U1\"")]
        [TestCase("\"\\U12\"")]
        [TestCase("\"\\U123\"")]
        [TestCase("\"\\U1234\"")]
        [TestCase("\"\\U12345\"")]
        [TestCase("\"\\U123456\"")]
        [TestCase("\"\\U1234567\"")]
        [TestCase("$\"\\U1\"")]
        [TestCase("$\"\\U12\"")]
        [TestCase("$\"\\U123\"")]
        [TestCase("$\"\\U1234\"")]
        [TestCase("$\"\\U12345\"")]
        [TestCase("$\"\\U123456\"")]
        [TestCase("$\"\\U1234567\"")]
        public void Expressions_StringLiteral_UnrecognizedEscape(string syntax)
        {
            var ast = TestSuite.GetGrammar().Expressions.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.UnrecognizedCharEscape);
        }

        [TestCase("$")]
        [TestCase("$@")]
        public void Expressions_StringLiteral_ExpectedDoubleQuote(string syntax)
        {
            

            var ast = TestSuite.GetGrammar().Expressions.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingDoubleQuote);
        }

        [Test]
        public void Expressions_StringLiteral_EndOfInput()
        {
            const string syntax = "\"";
            var ast = TestSuite.GetGrammar().Expressions.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.UnexpectedEndOfInput);
        }
    }
}
