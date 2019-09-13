using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlMergeDeployTool
{
    public class ReorderScriptPipeline : IAggregationPipeline
    {
        public string ProcessSql(List<string> sqls, CommandLineOptions opts)
        {
            var tableResult = string.Empty;
            var procResult = string.Empty;
            var funcResult = string.Empty;
            var tableTypeResult = string.Empty;
            var indexResult = string.Empty;
            var batchSeparator = Environment.NewLine + "GO" + Environment.NewLine;

            var parser = new TSql150Parser(false, SqlEngineType.All);

            foreach (var processedFragment in sqls)
            {
                var sqlReader = new StringReader(processedFragment);
                var fragment = parser.Parse(sqlReader, out var errors);
                if (errors.Any()) throw new Exception("Parsing failed");

                // TODO: Assumes exactly one statement per file, not ideal.
                var visitor = new ScriptTypeGetter();
                fragment.Accept(visitor);

                if (visitor.IsTable)
                {
                    tableResult += processedFragment + batchSeparator;
                }
                else if (visitor.IsProcedure)
                {
                    procResult += processedFragment + batchSeparator;
                }
                else if (visitor.IsFunction)
                {
                    funcResult += processedFragment + batchSeparator;
                }
                else if (visitor.IsTableType)
                {
                    tableTypeResult += processedFragment + batchSeparator;
                }
                else if (visitor.IsIndex)
                {
                    indexResult += processedFragment + batchSeparator;
                }
                else
                {
                    tableResult += processedFragment + batchSeparator;
                }
            }

            var result = tableResult + batchSeparator +
                         indexResult + batchSeparator +
                         tableTypeResult + batchSeparator +
                         funcResult + batchSeparator +
                         procResult + batchSeparator;

            return result;
        }
    }
}
