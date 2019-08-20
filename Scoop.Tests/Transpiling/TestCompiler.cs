using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using Scoop.SyntaxTree;
using Scoop.Transpiler;

namespace Scoop.Tests.Transpiling
{
    // Class to compile AST -> C# -> Assembly for test purposes only
    public static class TestCompiler
    {
        public static Assembly Compile(CompilationUnitNode ast)
        {
            var sb = new StringBuilder();
            new CSharpTranspileVisitor(sb).Visit(ast);
            var code = sb.ToString();
            return Compile(code);
        }

        public static Assembly Compile(string code)
        {
            var stackTrace = new StackTrace();
            string testMethodName = "";
            string testType = "";
            for (int i = 1; i < stackTrace.FrameCount; i++)
            {
                var frame = stackTrace.GetFrame(i);
                var method = frame.GetMethod();
                if (method.GetCustomAttribute<TestAttribute>() == null)
                    continue;
                testMethodName = method.Name;
                testType = method.DeclaringType.Namespace + "." + method.DeclaringType.Name;
                break;
            }

            var testAssemblyName = $"{testType}.{testMethodName}";
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create(testAssemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: new MetadataReference[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
                },
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );
            using (var ms = new MemoryStream())
            {
                var emitResult = compilation.Emit(ms);
                if (!emitResult.Success)
                {
                    var msg = string.Join("\n", emitResult.Diagnostics.Select(d => d.GetMessage()));
                    emitResult.Success.Should().BeTrue(msg);
                }

                ms.Seek(0, SeekOrigin.Begin);
                return Assembly.Load(ms.ToArray());
            }
        }
    }
}