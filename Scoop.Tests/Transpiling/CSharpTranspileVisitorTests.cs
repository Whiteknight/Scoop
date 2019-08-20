using System.Linq;
using System.Reflection;
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
    }
}
