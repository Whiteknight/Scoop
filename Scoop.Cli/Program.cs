using System;
using System.IO;

namespace Scoop.Cli
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            // TODO: Option to invoke roslyn and generate the .dll or .exe?
            // TODO: Option to dump to console instead of write to outfile?
            // TODO: Option to read from console?

            // TODO: Take layer in arglist
            var layer = ScoopTranspiler.GetLayer(LayerType.Layer1);
            if (args.Length == 0)
            {
                SearchFilesRecursively(layer, new DirectoryInfo(Environment.CurrentDirectory));
                return;
            }

            foreach (var file in args)
                TranspileFile(layer, file);
        }

        private static void SearchFilesRecursively(Layer layer, DirectoryInfo currentDirectory)
        {
            foreach (var file in currentDirectory.EnumerateFiles("*" + layer.FileExtension))
                TranspileFile(layer, file.FullName);
            foreach (var dir in currentDirectory.EnumerateDirectories())
            {
                if (dir.Name == "bin" || dir.Name == "obj" || dir.Name.StartsWith("."))
                    continue;
                SearchFilesRecursively(layer, dir);
            }
        }

        private static void TranspileFile(Layer layer, string fileName)
        {
            Console.WriteLine(fileName);
            var result = ScoopTranspiler.TranspileFile(layer.Type, fileName);
            if (!result.IsSuccess)
            {
                foreach (var error in result.Diagnostics)
                    Console.WriteLine("error: " + error);
                return;
            }
        }
    }
}
