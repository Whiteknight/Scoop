﻿using FluentAssertions;
using NUnit.Framework;
using Scoop.Parsing;
using Scoop.SyntaxTree;

namespace Scoop.Tests.L1.Validation
{
    [TestFixture]
    public class AttributeErrorsTests
    {
        [Test]
        public void Attribute_TargetMissingColon()
        {
            const string syntax = @"
[return MyAttr]";
            var ast = TestSuite.GetGrammar().Attributes.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingColon);
        }

        [Test]
        public void Attribute_MissingCloseParen()
        {
            const string syntax = @"
[MyAttr(]";
            var ast = TestSuite.GetGrammar().Attributes.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingCloseParen);
        }

        [Test]
        public void Attribute_MissingCloseBrace()
        {
            const string syntax = @"
[MyAttr";
            var ast = TestSuite.GetGrammar().Attributes.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
            result[0].ErrorMessage.Should().Be(Errors.MissingCloseBrace);
        }
    }
}
