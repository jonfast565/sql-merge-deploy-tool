using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlMergeDeployTool
{
    public interface IAggregationPipeline
    {
        string ProcessSql(List<string> sqls, CommandLineOptions opts);
    }
}
