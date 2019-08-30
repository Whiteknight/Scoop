using System;
using System.Collections.Generic;
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

        [Test]
        public void Compile_MethodReturnListOfInt()
        {
            var ast = new Parser().ParseUnit(@"
using System.Collections.Generic;

namespace XYZ 
{
    public class MyClass 
    {
        public List<int> MyMethod()
        {
            return new List<int>();
        }
    }
}");
            var assembly = TestCompiler.Compile(ast);
            var type = assembly.ExportedTypes.First();
            var myObj = Activator.CreateInstance(type);
            var method = type.GetMethod("MyMethod");
            var result = (List<int>)method.Invoke(myObj, new object[0]);
            result.Should().NotBeNull();
            result.Count.Should().Be(0);
        }

        [Test]
        public void Compile_MethodReturnListOfInt_Add()
        {
            var ast = new Parser().ParseUnit(@"
using System.Collections.Generic;

namespace XYZ 
{
    public class MyClass 
    {
        public List<int> MyMethod()
        {
            var list = new List<int>();
            list.Add(1);
            list.Add(3);
            list.Add(7);
            return list;
        }
    }
}");
            var assembly = TestCompiler.Compile(ast);
            var type = assembly.ExportedTypes.First();
            var myObj = Activator.CreateInstance(type);
            var method = type.GetMethod("MyMethod");
            var result = (List<int>)method.Invoke(myObj, new object[0]);
            result.Should().NotBeNull();
            result.Count.Should().Be(3);
            result[0].Should().Be(1);
            result[1].Should().Be(3);
            result[2].Should().Be(7);
        }

        [Test]
        public void Compile_MethodAddToListIndex()
        {
            var ast = new Parser().ParseUnit(@"
using System.Collections.Generic;

namespace XYZ 
{
    public class MyClass 
    {
        public int MyMethod()
        {
            var list = new List<int>();
            list.Add(1);
            list.Add(3);
            list.Add(7);
            return list[1];
        }
    }
}");
            var assembly = TestCompiler.Compile(ast);
            var type = assembly.ExportedTypes.First();
            var myObj = Activator.CreateInstance(type);
            var method = type.GetMethod("MyMethod");
            var result = (int)method.Invoke(myObj, new object[0]);
            result.Should().Be(3);
        }

        [Test]
        public void Compile_MethodReturnNullInvokeCoalesce()
        {
            var ast = new Parser().ParseUnit(@"
using System.Collections.Generic;

namespace XYZ 
{
    public class MyClass 
    {
        public string MyMethod()
        {
            return default(object)?.ToString() ?? ""ok"";
        }
    }
}");
            var assembly = TestCompiler.Compile(ast);
            var type = assembly.ExportedTypes.First();
            var myObj = Activator.CreateInstance(type);
            var method = type.GetMethod("MyMethod");
            var result = (string)method.Invoke(myObj, new object[0]);
            result.Should().Be("ok");
        }

        [Test]
        public void Compile_MethodReturnLambdaInvoke()
        {
            var ast = new Parser().ParseUnit(@"
using System;
using System.Collections.Generic;

namespace XYZ 
{
    public class MyClass 
    {
        public int MyMethod()
        {
            c# { Func<int, int> func; }   
            func = (a => a + 5);
            return func(4);
        }
    }
}");
            var assembly = TestCompiler.Compile(ast);
            var type = assembly.ExportedTypes.First();
            var myObj = Activator.CreateInstance(type);
            var method = type.GetMethod("MyMethod");
            var result = (int)method.Invoke(myObj, new object[0]);
            result.Should().Be(9);
        }

        [Test]
        public void Compile_CSharpCodeLiterals()
        {
            var ast = new Parser().ParseUnit(@"
using System.Collections.Generic;

namespace XYZ 
{
    public class MyClass 
    {
        // use code literal to define a protected method, which Scoop doesn't support
        c# 
        {
            protected int CSharpMethod() => 4;
        }

        public int MyMethod()
        {
            var v = CSharpMethod();
            // Use code literal to use an if statement
            c# {
                if (v == 4)
                    v = 6;
            };
            return v;
        }
    }
}");
            var assembly = TestCompiler.Compile(ast);
            var type = assembly.ExportedTypes.First();
            var myObj = Activator.CreateInstance(type);
            var method = type.GetMethod("MyMethod");
            var result = (int)method.Invoke(myObj, new object[0]);
            result.Should().Be(6);
        }
    }
}
