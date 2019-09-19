using System;
using System.IO;

namespace Scoop.Cli
{
    class Program
    {
        private const string _extension = ".scl1";

        static void Main(string[] args)
        {
            // TODO: Option to invoke roslyn and generate the .dll or .exe?
            // TODO: Option to dump to console instead of write to outfile?
            // TODO: Option to read from console?
            if (args.Length == 0)
            {
                SearchFilesRecursively(new DirectoryInfo(Environment.CurrentDirectory));
                return;
            }

            foreach (var file in args)
                TranspileFile(file);
        }

        static void SearchFilesRecursively(DirectoryInfo currentDirectory)
        {
            foreach (var file in currentDirectory.EnumerateFiles("*" + _extension))
                TranspileFile(file.FullName);
            foreach (var dir in currentDirectory.EnumerateDirectories())
            {
                if (dir.Name == "bin" || dir.Name == "obj" || dir.Name.StartsWith("."))
                    continue;
                SearchFilesRecursively(dir);
            }
        }

        static void TranspileFile(string fileName)
        {
            Console.WriteLine(fileName);
            var result = ScoopTranspiler.TranspileFile(fileName);
            if (!result.IsSuccess)
            {
                foreach (var error in result.Diagnostics)
                    Console.WriteLine("error: " + error);
                return;
            }
        }
    }
}
