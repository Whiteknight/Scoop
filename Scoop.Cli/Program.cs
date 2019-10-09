using System;
using System.IO;

namespace Scoop.Cli
{
    public static class Program
    {
        private const string _fileExtension = ".sc";

        private static void Main(string[] args)
        {
            // TODO: Option to invoke roslyn and generate the .dll or .exe?
            // TODO: Option to dump to console instead of write to outfile?
            // TODO: Option to read from console?

            // TODO: Get list of features from the commandline and turn them on
            var transpiler = new ScoopTranspiler();
            if (args.Length == 0)
            {
                SearchFilesRecursively(transpiler, new DirectoryInfo(Environment.CurrentDirectory));
                return;
            }

            foreach (var file in args)
                TranspileFile(transpiler, file);
        }

        private static void SearchFilesRecursively(ScoopTranspiler transpiler, DirectoryInfo currentDirectory)
        {
            foreach (var file in currentDirectory.EnumerateFiles("*" + _fileExtension))
                TranspileFile(transpiler, file.FullName);
            foreach (var dir in currentDirectory.EnumerateDirectories())
            {
                if (dir.Name == "bin" || dir.Name == "obj" || dir.Name.StartsWith("."))
                    continue;
                SearchFilesRecursively(transpiler, dir);
            }
        }

        private static void TranspileFile(ScoopTranspiler transpiler, string fileName)
        {
            Console.WriteLine(fileName);
            var result = transpiler.TranspileFile(fileName);
            if (!result.IsSuccess)
            {
                foreach (var error in result.Diagnostics)
                    Console.WriteLine("error: " + error);
                return;
            }
        }
    }
}
