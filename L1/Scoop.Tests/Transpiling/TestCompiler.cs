using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
        public static Assembly Compile(ClassNode ast)
        {
            var testAssemblyName = GetTestAssemblyName();
            var unit = new CompilationUnitNode
            {
                Members = new ListNode<AstNode>
                {
                    new NamespaceNode
                    {
                        Name = new DottedIdentifierNode(testAssemblyName),
                        Declarations = new ListNode<AstNode>
                        {
                            ast
                        }
                    }
                }
            };
            var code = CSharpTranspileVisitor.ToString(unit);
            return Compile(code, testAssemblyName);
        }

        public static Assembly Compile(CompilationUnitNode ast)
        {
            var errors = ast.Validate();
            errors.Count.Should().Be(0);
            var code = CSharpTranspileVisitor.ToString(ast);
            Debug.WriteLine(code);
            var testAssemblyName = GetTestAssemblyName();
            return Compile(code, testAssemblyName);
        }

        public static Assembly Compile(string code, string testAssemblyName)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Diagnostics.Debug).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IEnumerable<>).Assembly.Location)
            };
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.StartsWith("System")))
                references.Add(MetadataReference.CreateFromFile(assembly.Location));

            var compilation = CSharpCompilation.Create(testAssemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
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

        private static string GetTestAssemblyName()
        {
            var stackTrace = new StackTrace();
            for (int i = 1; i < stackTrace.FrameCount; i++)
            {
                var frame = stackTrace.GetFrame(i);
                var method = frame.GetMethod();
                if (method.GetCustomAttribute<TestAttribute>() == null)
                    continue;
                return $"{method.DeclaringType.Namespace}.{method.DeclaringType.Name}.{method.Name}";
            }

            throw new Exception("Could not find [Test] method in stack trace");
        }
    }
}