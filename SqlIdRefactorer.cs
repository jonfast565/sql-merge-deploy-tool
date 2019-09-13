using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlMergeDeployTool
{
    internal class SqlIdRefactorer : TSqlConcreteFragmentVisitor
    {
        public List<SqlIdPosition> IdPositions { get; set; } = new List<SqlIdPosition>();

        public override void ExplicitVisit(MultiPartIdentifierCallTarget callTarget)
        {
            var ids = callTarget.MultiPartIdentifier.Identifiers.Select(x => x.Value);
            if (!ids.Contains("dbo"))
                return;

            var startLocation = callTarget.StartOffset;
            var endLocation = callTarget.StartOffset + callTarget.FragmentLength;
            var target = ids.ToArray()[0];

            var idPosition = new SqlIdPosition
            {
                Schema = target,
                TableName = null,
                StartPosition = startLocation,
                EndPosition = endLocation
            };

            IdPositions.Add(idPosition);
            Console.WriteLine($"[Found CallTgt] {target} ({startLocation}, {endLocation})");
        }

        public override void ExplicitVisit(SchemaObjectNameOrValueExpression valueObj)
        {
            var node = valueObj.SchemaObjectName;
            ExplicitVisit(node);
        }

        public override void ExplicitVisit(SchemaObjectName node)
        {

            if (node.Identifiers.Count() < 1 || node.Identifiers.Count() > 2)
                return;

            var names = Array.ConvertAll((SqlDbType[])Enum.GetValues(typeof(SqlDbType)),
                             type => type.ToString().ToUpper());

            var id1 = node.Identifiers[0].Value;

            if (names.Contains(id1))
                return;

            var id2 = node.Identifiers.Count() == 2 ? node.Identifiers[1]?.Value : null;
            if (id1 != "dbo" && id2 == null)
            {
                id2 = id1;
                id1 = "dbo";
            }

            var startLocation = node.StartOffset;
            var endLocation = node.StartOffset + node.FragmentLength;
            var ids = node.Identifiers;
            var collapsedIdentifier = ids.Aggregate(string.Empty, 
                (s, identifier) => s + identifier.Value + ".").TrimEnd('.');

            var idPosition = new SqlIdPosition
            {
                Schema = id1,
                TableName = id2,
                StartPosition = startLocation,
                EndPosition = endLocation
            };

            IdPositions.Add(idPosition);
            Console.WriteLine($"[Found Id] {collapsedIdentifier} ({startLocation}, {endLocation})");
        }

        public string ProcessIdChanges(string frag, string schemaName)
        {
            var currentPositions = IdPositions.OrderBy(x => x.StartPosition).ToArray();
            if (currentPositions.Count() == 0)
            {
                return frag;
            }
            else
            {
                var result = string.Empty;
                for (var i = 0; i < currentPositions.Count(); i++)
                {
                    if (i == 0)
                    {
                        result = BeginString(frag, schemaName, currentPositions, result, i);
                        if (i == currentPositions.Count() - 1)
                        {
                            result = EndString(frag, currentPositions, result, i);
                        }
                    }
                    else if (i == currentPositions.Count() - 1)
                    {
                        result = MiddleString(frag, schemaName, currentPositions, result, i);
                        result = EndString(frag, currentPositions, result, i);
                    }
                    else
                    {
                        result = MiddleString(frag, schemaName, currentPositions, result, i);
                    }
                }
                return result;
            }
        }

        private static string BeginString(string frag, string schemaName, SqlIdPosition[] currentPositions, string result, int i)
        {
            var fragBeginPos = new Tuple<int, int>(0, currentPositions[i].StartPosition);
            var substr = frag.Position(fragBeginPos) + currentPositions[i].GetIdString(schemaName);
            result += substr;
            return result;
        }

        private static string EndString(string frag, SqlIdPosition[] currentPositions, string result, int i)
        {
            var fragEndPos = new Tuple<int, int>(currentPositions[i].EndPosition, frag.Length);
            var substr = frag.Position(fragEndPos);
            result += substr;
            return result;
        }

        private static string MiddleString(string frag, string schemaName, SqlIdPosition[] currentPositions, string result, int i)
        {
            var fragMidPos = new Tuple<int, int>(currentPositions[i - 1].EndPosition, currentPositions[i].StartPosition);
            var substr = frag.Position(fragMidPos) + currentPositions[i].GetIdString(schemaName);
            result += substr;
            return result;
        }
    }
}
