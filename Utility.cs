using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlMergeDeployTool
{
    public static class Utility
    {
        public static string Repeat(this string toCopy, int numberOfTimes)
        {
            var result = string.Empty;
            for (var i = 0; i < numberOfTimes; i++)
            {
                result += toCopy;
            }

            return result;
        }

        public static string Position(this string toPosition, int startIndex, int endIndex)
        {
            try
            {
                var newStartIndex = Math.Min(startIndex, endIndex);
                var length = Math.Abs(startIndex - endIndex);
                var substr = toPosition.Substring(newStartIndex, length);
                return substr;
            }
            catch (ArgumentOutOfRangeException e)
            {
                throw new Exception($"String position ({startIndex}, {endIndex}) failed with improper length", e);
            }
        }

        public static string Position(this string toPosition, Tuple<int, int> position)
        {
            return toPosition.Position(position.Item1, position.Item2);
        }

        public static string StringPreview(this string sql)
        {
            var firstLines = sql.Split(Environment.NewLine.ToCharArray())[0];
            return firstLines;
        }
    }
}
