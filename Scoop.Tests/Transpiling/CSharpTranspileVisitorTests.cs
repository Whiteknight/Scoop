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
    }
}
