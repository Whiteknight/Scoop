using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Scoop.Parsers;

namespace Scoop.Tests.L1.Transpiling
{
    [TestFixture]
    public class CSharpTranspileVisitorTests
    {
        [Test]
        public void Compile_EmptyNamespace()
        {
            var ast = TestSuite.GetScoopGrammar().CompilationUnits.Parse(@"
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
            var ast = TestSuite.GetScoopGrammar().Classes.Parse(@"

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
            var ast = TestSuite.GetScoopGrammar().Classes.Parse(@"
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
            var ast = TestSuite.GetScoopGrammar().Classes.Parse(@"
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
            var ast = TestSuite.GetScoopGrammar().CompilationUnits.Parse(@"
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
            var ast = TestSuite.GetScoopGrammar().CompilationUnits.Parse(@"
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
            var ast = TestSuite.GetScoopGrammar().CompilationUnits.Parse(@"
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
            var ast = TestSuite.GetScoopGrammar().CompilationUnits.Parse(@"
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
            var ast = TestSuite.GetScoopGrammar().CompilationUnits.Parse(@"
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
            var ast = TestSuite.GetScoopGrammar().CompilationUnits.Parse(@"
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

        [Test]
        public void Compile_MethodGenericExtensionMethod()
        {
            var ast = TestSuite.GetScoopGrammar().CompilationUnits.Parse(@"
using System.Collections.Generic;
using System.Linq;

namespace XYZ 
{
    public class MyClass 
    {
        public int MyMethod(List<List<int>> e)
        {
            return e.First<List<int>>().First<int>();
        }
    }
}");
            var assembly = TestCompiler.Compile(ast);
            var type = assembly.ExportedTypes.First();
            var myObj = Activator.CreateInstance(type);
            var method = type.GetMethod("MyMethod");
            var result = (int)method.Invoke(myObj, new object[] { new List<List<int>> { new List<int> { 5 } } });
            result.Should().Be(5);
        }

        [Test]
        public void Compile_Initializers()
        {
            var ast = TestSuite.GetScoopGrammar().CompilationUnits.Parse(@"
using System.Collections.Generic;
using System.Linq;

namespace XYZ 
{
    public class MyClass 
    {
        c# {
            public class TestClass {
                public int Value { get; set; }
            }
        }

        public List<object> MyMethod()
        {
            return new List<object> {
                new Dictionary<int, string> { { 1, ""test"" } },
                new TestClass() { Value = true ? 5 : 4 }
            };
        }
    }
}");

            var assembly = TestCompiler.Compile(ast);
            var type = assembly.ExportedTypes.First();
            var myObj = Activator.CreateInstance(type);
            var method = type.GetMethod("MyMethod");
            var result = (List<object>)method.Invoke(myObj, new object[0]);
            result.Count.Should().Be(2);
            (result[0] as Dictionary<int, string>).Should().BeEquivalentTo(new Dictionary<int, string> { { 1, "test" } });
            var innerObj = result[1];
            var innerObjType = innerObj.GetType();
            var prop = innerObjType.GetProperty("Value");
            var intValue = (int) prop.GetValue(innerObj);
            intValue.Should().Be(5);

            // TODO: Indexer initializer syntax "[0] = ..." parses and serializes correctly but I can't
            // find a test scenario which actually works in Roslyn.
        }

        [Test]
        public void Compile_MethodIndexerInitializer()
        {
            var ast = TestSuite.GetScoopGrammar().CompilationUnits.Parse(@"
using System.Collections.Generic;
using System.Linq;

namespace XYZ 
{
    public class MyClass 
    {
        public int MyMethod()
        {
            var list = new MyList { [0] = 5 };
            return list[0];
        }
    }


    private class MyList
    {
        List<int> Items;
        public MyList() { Items = new List<int>(); }
        c#{
            public int this[int i] 
            {
                get { return Items[i]; }
                set { Items.Insert(i, value); }
            }
        }
    }
}");
            var assembly = TestCompiler.Compile(ast);
            var type = assembly.ExportedTypes.First();
            var myObj = Activator.CreateInstance(type);
            var method = type.GetMethod("MyMethod");
            var result = (int)method.Invoke(myObj, new object[] { });
            result.Should().Be(5);
        }

        [Test]
        public void Compile_ArrayType()
        {
            var ast = TestSuite.GetScoopGrammar().CompilationUnits.Parse(@"
using System.Collections.Generic;
using System.Linq;

namespace XYZ 
{
    public class MyClass 
    {
        private const int _two = 2;
        public int[] MyMethod()
        {
            return new int[] { 1, _two, 3 }; 
        }
    }
}");
            var assembly = TestCompiler.Compile(ast);
            var type = assembly.ExportedTypes.First();
            var myObj = Activator.CreateInstance(type);
            var method = type.GetMethod("MyMethod");
            var result = (int[])method.Invoke(myObj, new object[] { });
            result.Should().BeEquivalentTo(1, 2, 3);
        }

        [Test]
        public void Compile_FlagsEnum()
        {
            var ast = TestSuite.GetScoopGrammar().CompilationUnits.Parse(@"
using System;
using System.Collections.Generic;
using System.Linq;

namespace XYZ 
{
    [Flags]
    enum MyEnum
    {
        A, B = 1, C
    }

    public class MyClass 
    {
        public int MyMethod()
        {
            return (int)MyEnum.B;
        }
    }
}");
            var assembly = TestCompiler.Compile(ast);
            var type = assembly.ExportedTypes.First();
            var myObj = Activator.CreateInstance(type);
            var method = type.GetMethod("MyMethod");
            var result = (int)method.Invoke(myObj, new object[] { });
            result.Should().Be(1);
        }

        [Test]
        public void Compile_InterfaceInherit()
        {
            var ast = TestSuite.GetScoopGrammar().CompilationUnits.Parse(@"
using System;
using System.Collections.Generic;
using System.Linq;

namespace XYZ 
{
    public interface IMyInterface
    {
        int MyMethod();
    }

    public class MyClass : IMyInterface
    {
        public int MyMethod() => 1;
    }
}");
            var assembly = TestCompiler.Compile(ast);
            var type = assembly.ExportedTypes.First(t => t.Name == "MyClass");
            type.GetInterface("IMyInterface").Should().NotBeNull();
        }

        [Test]
        public void Compile_MethodParameters()
        {
            var ast = TestSuite.GetScoopGrammar().Classes.Parse(@"
public class MyClass 
{
    public int MyMethod()
    {
        return PrivateMethod(4, c: 5);
    }

    private int PrivateMethod(int a = 0, int b = 1, int c = 2)
    {
        return (a * 100) + (b * 10) + c;
    }
}");

            var assembly = TestCompiler.Compile(ast);
            var type = assembly.ExportedTypes.First();
            var myObj = Activator.CreateInstance(type);
            var method = type.GetMethod("MyMethod");
            var result = (int)method.Invoke(myObj, new object[0]);
            result.Should().Be(415);
        }

        [Test]
        public void Compile_ExpressionPrefixPostfix()
        {
            var ast = TestSuite.GetScoopGrammar().Classes.Parse(@"
public class MyClass 
{
    public int MyMethod()
    {
        var x = 5;
        var y = ++x;
        x++;
        return x * 10 + y;
    }
}");

            var assembly = TestCompiler.Compile(ast);
            var type = assembly.ExportedTypes.First();
            var myObj = Activator.CreateInstance(type);
            var method = type.GetMethod("MyMethod");
            var result = (int)method.Invoke(myObj, new object[0]);
            result.Should().Be(76);
        }
    }
}
