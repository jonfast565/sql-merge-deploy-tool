using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CommandLine;
using CommandLine.Text;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PoorMansTSqlFormatterLib.Formatters;

namespace SqlMergeDeployTool
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            Header.PrintHeader();
            var returnCode = Parser.Default.ParseArguments<CommandLineOptions>(args)
                .MapResult(RunOptionsAndReturnExitCode, errs => 1);

            Header.PrintGoodbye();
            return returnCode;
        }

        private static int RunOptionsAndReturnExitCode(CommandLineOptions opts)
        {
            Console.WriteLine($"Using directory: {opts.RootDirectory}");

            try
            {
                var projectFiles = GetFiles(opts.RootDirectory, ".sqlproj", new[] { "bin", "obj" });
                foreach (var projectFile in projectFiles)
                {
                    ProcessProjectFile(projectFile, opts);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unhandled exception. Stopping.");
                Console.WriteLine(e.Message);
                return -1;
            }

            return 0;
        }

        private static string[] GetInternalProjectFileList(string projectFile)
        {
            var projectXDoc = XDocument.Load(new XmlTextReader(projectFile));
            var namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("e", "http://schemas.microsoft.com/developer/msbuild/2003");
            var projectBuildItems = projectXDoc.XPathSelectElements("/e:Project/e:ItemGroup/e:Build", namespaceManager);
            var results = projectBuildItems.Select(x => x.Attribute("Include")?.Value).ToArray();
            return results;
        }

        private static string GetParentDirectory(string fileString)
        {
            var info = new DirectoryInfo(fileString);
            return info.Parent?.FullName;
        }

        private static void ProcessProjectFile(string projectFile, CommandLineOptions opts)
        {
            var files = GetInternalProjectFileList(projectFile);
            var projectParentPath = GetParentDirectory(projectFile);

            var processedSql = new List<string>();
            foreach (var file in files)
            {
                var projectPath = Path.Combine(projectParentPath, file);
                var sql = File.ReadAllText(projectPath);
                var sqlPreview = sql.StringPreview();
                Console.WriteLine($"[Parsing] {sqlPreview}...");
                var processingStep = new ChangeSchemaPipeline();
                var result = processingStep.ProcessSql(sql, opts);
                processedSql.Add(result);
            }

            var aggregationStep = new ReorderScriptPipeline();
            var finalSql = aggregationStep.ProcessSql(processedSql, opts);
            VerificationParse(true, finalSql);
            var formattedResult = FormatSql(finalSql);
            File.WriteAllText("./results.sql", formattedResult);
        }

        private static void VerificationParse(bool enable, string finalSql)
        {
            var parser = new TSql150Parser(false, SqlEngineType.All);
            if (!enable) return;

            var resultReader = new StringReader(finalSql);
            parser.Parse(resultReader, out var finalErrors);
            if (!finalErrors.Any()) return;

            PrintParserErrors(finalErrors);
            throw new Exception("Result of the SQL file is not valid. This should not happen.");
        }

        private static string FormatSql(string sql)
        {
            Console.WriteLine($"[Formatting] resulting SQL code.");
            var options = new TSqlStandardFormatterOptions
            {
                KeywordStandardization = true,
                IndentString = "\t",
                SpacesPerTab = 4,
                MaxLineWidth = 999,
                NewStatementLineBreaks = 2,
                NewClauseLineBreaks = 1,
                TrailingCommas = false,
                SpaceAfterExpandedComma = false,
                ExpandBetweenConditions = true,
                ExpandBooleanExpressions = true,
                ExpandCaseStatements = true,
                ExpandCommaLists = true,
                BreakJoinOnSections = true,
                UppercaseKeywords = true,
                ExpandInLists = true
            };

            var parsingError = false;
            var formatter = new TSqlStandardFormatter(options);
            var formattingManager = new PoorMansTSqlFormatterLib.SqlFormattingManager(formatter);
            var formattedOutput = formattingManager.Format(sql, ref parsingError);
            return formattedOutput;
        }

        private static IEnumerable<string> GetFiles(string rootDirectory, string extension, string[] skipFolders)
        {
            var files = Directory.GetFiles(rootDirectory, "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(extension))
                .Where(s => skipFolders.Aggregate(true, (current, folder) => current & !s.Contains($"\\{folder}\\")));
            return files;
        }

        private static void PrintParserErrors(IList<ParseError> errors)
        {
            if (!errors.Any()) return;

            foreach (var error in errors)
            {
                Console.WriteLine($"[Error @ {error.Line}:{error.Column}] {error.Message}");
            }
        }
    }
}
