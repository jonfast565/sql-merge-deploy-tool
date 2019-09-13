using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlMergeDeployTool
{
    public class CommandLineOptions
    {
        [Value(0,
            MetaName = "rootDir",
            HelpText = "Runs this program in a directory that is not its current directory. If left blank, is the current directory.")]
        public string RootDirectory { get; set; }

        [Value(1,
            MetaName = "newSchema",
            HelpText = "A schema that the script transforms into")]

        public string NewSchema { get; internal set; }

        public CommandLineOptions()
        {
            if (string.IsNullOrEmpty(RootDirectory))
            {
                RootDirectory = Environment.CurrentDirectory;
            }

            if (string.IsNullOrEmpty(NewSchema))
            {
                NewSchema = "dbo";
            }
        }
    }
}
