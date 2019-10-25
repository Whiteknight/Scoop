using FluentAssertions;
using NUnit.Framework;
using Scoop.Parsing;
using Scoop.SyntaxTree;

namespace Scoop.Tests.L1.Validation
{
    [TestFixture]
    public class InterfaceErrorsTests 
    {
        [Test]
        public void InterfaceMember_MissingName()
        {
            const string syntax = @"
public interface  { 
    int MyMethod();
}";
            var ast = TestSuite.GetGrammar().Interfaces.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingIdentifier);
        }

        [Test]
        public void InterfaceMember_MissingOpenBracket()
        {
            const string syntax = @"
public interface MyInterface 
    int MyMethod();
}";
            var ast = TestSuite.GetGrammar().Interfaces.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingOpenBracket);
        }

        [Test]
        public void InterfaceMember_MissingCloseBracket()
        {
            const string syntax = @"
public interface MyInterface { 
    int MyMethod();
";
            var ast = TestSuite.GetGrammar().Interfaces.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingCloseBracket);
        }

        [Test]
        public void InterfaceMember_MissingOpenParen()
        {
            const string syntax = @"
public interface MyInterface { 
    int MyMethod);
}";
            var ast = TestSuite.GetGrammar().Interfaces.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            // we can't do anything about ordering here
            result[0].ErrorMessage.Should().Be(Errors.MissingOpenParen);
        }

        [Test]
        public void InterfaceMember_MissingCloseParen()
        {
            const string syntax = @"
public interface MyInterface { 
    int MyMethod(;
}";
            var ast = TestSuite.GetGrammar().Interfaces.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingCloseParen);
        }

        [Test]
        public void InterfaceMember_MissingSemicolon()
        {
            const string syntax = @"
public interface MyInterface { 
    int MyMethod()
}";
            var ast = TestSuite.GetGrammar().Interfaces.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingSemicolon);
        }

        [Test]
        public void InterfaceMember_MissingInheritedType()
        {
            const string syntax = @"
public interface MyInterface : 
{ 
}";
            var ast = TestSuite.GetGrammar().Interfaces.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingType);
        }
    }
}
