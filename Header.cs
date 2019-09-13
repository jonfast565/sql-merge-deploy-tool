using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SqlMergeDeployTool
{
    public static class Header
    {
        public static void PrintHeader()
        {
            Console.WriteLine("-- SQL Database Project Transform --");
            Console.WriteLine("Author: Jon Fast");
            Console.WriteLine($"Version: {Assembly.GetEntryAssembly()?.GetName().Version}");
            Console.WriteLine("--");
            Console.WriteLine();
        }

        public static void PrintGoodbye()
        {
            Console.WriteLine();
            Console.WriteLine("--");
            Console.WriteLine("Done! Goodbye.");
        }
    }
}
