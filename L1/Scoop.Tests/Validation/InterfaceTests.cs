using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Scoop.SyntaxTree;
using Scoop.Tests.Utility;
using Scoop.Validation;

namespace Scoop.Tests.Validation
{
    [TestFixture]
    public class InterfaceTests 
    {
        [Test]
        public void InterfaceMember_MissingSemicolon()
        {
            const string syntax = @"
public interface MyInterface { 
    int MyMethod()
}";
            var ast = new Parser().Interfaces.Parse(syntax);
            var result = ast.Validate();
            result.Count.Should().Be(1);
        }
    }
}
