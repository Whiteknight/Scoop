using FluentAssertions;
using NUnit.Framework;
using Scoop.Parsing;
using Scoop.SyntaxTree;

namespace Scoop.Tests.L1.Validation
{
    [TestFixture]
    public class StatementsErrorsTests
    {
        [Test]
        public void Const_MissingIdentifier()
        {
            // Case where type is missing is indistinguishable. Always comes back as missing identifier
            const string syntax = @"
const int = 1;";
            var ast = TestSuite.GetGrammar().Statements.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingIdentifier);
        }

        [Test]
        public void Const_MissingEquals()
        {
            const string syntax = @"
const int x  1;";
            var ast = TestSuite.GetGrammar().Statements.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingEquals);
        }

        [Test]
        public void Const_MissingExpression()
        {
            const string syntax = @"
const int x = ;";
            var ast = TestSuite.GetGrammar().Statements.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingExpression);
        }

        [Test]
        public void Const_MissingSemicolon()
        {
            const string syntax = @"
const int x = 1";
            var ast = TestSuite.GetGrammar().Statements.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingSemicolon);
        }

        [Test]
        public void VarDeclare_MissingEquals()
        {
            const string syntax = @"
int x  1;";
            var ast = TestSuite.GetGrammar().Statements.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingSemicolon);
        }

        [Test]
        public void VarDeclare_MissingExpression()
        {
            const string syntax = @"
int x = ;";
            var ast = TestSuite.GetGrammar().Statements.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingSemicolon);
        }

        [Test]
        public void Return_MissingSemicolon1()
        {
            const string syntax = @"
return";
            var ast = TestSuite.GetGrammar().Statements.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingSemicolon);
        }

        [Test]
        public void Return_MissingSemicolon2()
        {
            const string syntax = @"
return 1";
            var ast = TestSuite.GetGrammar().Statements.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingSemicolon);
        }

        [Test]
        public void UsingStmt_MissingOpenParen()
        {
            const string syntax = @"
using x) x.DoWork();";
            var ast = TestSuite.GetGrammar().Statements.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingOpenParen);
        }

        [Test]
        public void UsingStmt_MissingExpression()
        {
            const string syntax = @"
using() x.DoWork();";
            var ast = TestSuite.GetGrammar().Statements.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingExpression);
        }

        [Test]
        public void UsingStmt_MissingCloseParen()
        {
            const string syntax = @"
using(x x.DoWork();";
            var ast = TestSuite.GetGrammar().Statements.Parse(syntax);
            var result = ast.Validate();
            // There are actually other messages which come out here, so don't assert the count
            result[0].ErrorMessage.Should().Be(Errors.MissingCloseParen);
        }

        [Test]
        public void UsingStmt_MissingSemicolon()
        {
            const string syntax = @"
using(x) x.DoWork()";
            var ast = TestSuite.GetGrammar().Statements.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingSemicolon);
        }

        [Test]
        public void ExpressionStmt_MissingSemicolon()
        {
            const string syntax = @"
DoWork()";
            var ast = TestSuite.GetGrammar().Statements.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingSemicolon);
        }
    }
}
