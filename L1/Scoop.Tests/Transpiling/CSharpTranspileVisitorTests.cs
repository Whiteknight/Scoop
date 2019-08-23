using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Scoop.Tests.Transpiling
{
    [TestFixture]
    public class CSharpTranspileVisitorTests
    {
        [Test]
        public void Compile_EmptyNamespace()
        {
            var ast = new Parser().ParseUnit(@"
namespace XYZ
{
    public class MyClass { }
}");
            var assembly = TestCompiler.Compile(ast);
            assembly.Should().NotBeNull();
            assembly.ExportedTypes.First().Name.Should().Be("MyClass");
        }

        [Test]
        public void Compile_ClassCtorAndMethod()
        {
            var ast = new Parser().ParseClass(@"

public class MyClass 
{
    // default parameterless constructor
    public MyClass() { }

    // Simple void() method
    public void MyMethod() { }
}");

            var assembly = TestCompiler.Compile(ast);
            assembly.Should().NotBeNull();

            var type = assembly.ExportedTypes.First();
            type.Name.Should().Be("MyClass");

            type.GetConstructors().Length.Should().Be(1);
            var ctor = type.GetConstructors().First();
            ctor.GetParameters().Length.Should().Be(0);

            var method = type.GetMethod("MyMethod");
            method.Name.Should().Be("MyMethod");
            method.ReturnType.Should().Be(typeof(void));
            method.GetParameters().Length.Should().Be(0);
        }

        [Test]
        public void Compile_ClassMethodReturnValue()
        {
            var ast = new Parser().ParseClass(@"
public class MyClass 
{
    public int MyMethod()
    {
        return 5 + 6;
    }
}");

            var assembly = TestCompiler.Compile(ast);
            var type = assembly.ExportedTypes.First();
            var myObj = Activator.CreateInstance(type);
            var method = type.GetMethod("MyMethod");
            var result = (int) method.Invoke(myObj, new object[0]);
            result.Should().Be(11);
        }

        [Test]
        public void Compile_ClassMethodReturnExpression()
        {
            var ast = new Parser().ParseClass(@"
public class MyClass 
{
    public string MyMethod()
    {
        return 5.ToString() + ""test"".Length;
    }
}");

            var assembly = TestCompiler.Compile(ast);
            var type = assembly.ExportedTypes.First();
            var myObj = Activator.CreateInstance(type);
            var method = type.GetMethod("MyMethod");
            var result = (string)method.Invoke(myObj, new object[0]);
            result.Should().Be("54");
        }
    }
}
