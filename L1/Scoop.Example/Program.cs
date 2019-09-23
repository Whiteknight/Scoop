using System;

namespace Scoop.Example
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            // If "MyClass" cannot be found, it's because the MyClass.scl1.cs file didn't correctly generate
            var myObj = new MyClass();
        }
    }
}
