using System;
using System.Linq;

namespace Scoop.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: Option to invoke roslyn and generate the .dll or .exe?
            // TODO: Option to dump to console instead of write to outfile?
            // TODO: Option to read from console?
            // TODO: compile all args, and handle wildcards?
            // TODO: Prepend each file with a header to warn about code being generated
            if (args.Length == 0)
            {
                Console.WriteLine("At least one input file expected");
                return;
            }

            var result = ScoopTranspiler.TranspileFile(args.First());
            if (!result.IsSuccess)
            {
                foreach (var error in result.Diagnostics)
                    Console.WriteLine(error);
                return;
            }
        }
    }
}
