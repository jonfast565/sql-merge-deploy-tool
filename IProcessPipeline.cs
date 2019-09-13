using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlMergeDeployTool
{
    public interface IProcessPipeline
    {
        string ProcessSql(string sql, CommandLineOptions opts);
    }
}
