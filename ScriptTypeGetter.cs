using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlMergeDeployTool
{
    public class ScriptTypeGetter : TSqlConcreteFragmentVisitor
    {
        public bool IsTable { get; set; }
        public bool IsProcedure { get; set; }
        public bool IsFunction { get; set; }
        public bool IsIndex { get; set; }
        public bool IsTableType { get; set; }

        public override void ExplicitVisit(CreateTableStatement node)
        {
            IsTable = true;
        }

        public override void ExplicitVisit(CreateProcedureStatement node)
        {
            IsProcedure = true;
        }

        public override void ExplicitVisit(CreateFunctionStatement node)
        {
            IsFunction = true;
        }

        public override void ExplicitVisit(CreateTypeTableStatement node)
        {
            IsTableType = true;
        }

        public override void ExplicitVisit(CreateIndexStatement node)
        {
            IsIndex = true;
        }
    }
}
