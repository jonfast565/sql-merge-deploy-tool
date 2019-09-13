using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlMergeDeployTool
{
    public class ChangeSchemaPipeline : IProcessPipeline
    {
        public string ProcessSql(string sql, CommandLineOptions opts)
        {
            var newSchema = opts.NewSchema;
            var parser = new TSql150Parser(false, SqlEngineType.All);

            var sqlReader = new StringReader(sql);
            var fragment = parser.Parse(sqlReader, out var errors);
            if (errors.Any()) throw new Exception("Parsing failed");

            var script = fragment as TSqlScript;
            var idRefactorer = new SqlIdRefactorer();

            Debug.Assert(script?.Batches != null, "script?.Batches != null");
            script.AcceptChildren(idRefactorer);

            var newScript = idRefactorer.ProcessIdChanges(sql, newSchema);
            // Console.WriteLine($"[Done] {newScript.StringPreview()}...");

            return newScript;
        }
    }
}
