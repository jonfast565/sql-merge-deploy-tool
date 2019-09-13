using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlMergeDeployTool
{
    public class ConstraintGetter : TSqlConcreteFragmentVisitor
    {
        public override void ExplicitVisit(CreateTableStatement node)
        {
            Console.WriteLine(node);
            var tableName = node.SchemaObjectName;
            
        }
    }
}
