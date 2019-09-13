using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlMergeDeployTool
{
    public class SqlIdPosition
    {
        public string Schema { get; set; }
        public string TableName { get; set; }
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }

        internal string GetIdString(string schemaName)
        {
            return TableName != null ? 
                $"[{schemaName ?? Schema}].[{TableName}]" : 
                $"[{schemaName ?? Schema}]";
        }
    }
}
