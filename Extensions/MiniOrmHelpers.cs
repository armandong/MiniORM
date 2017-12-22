using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MiniORM
{
    public static class MiniOrmHelpers
    {
        public static ExecuteType CheckExecuteType(string query)
        {
            ExecuteType executeType = ExecuteType.NONE;

            Func<string, bool> IsMatch = pattern => 
            {
                if (!Regex.IsMatch(query, pattern, RegexOptions.IgnoreCase))
                    return false;

                return true;
            };

            if (IsMatch(@"(?=.*insert)(?=.*into)(?=.*values)"))
                executeType = ExecuteType.INSERT;
            else if (IsMatch(@"(?=.*update)(?=.*set)"))
                executeType = ExecuteType.UPDATE;
            else if (IsMatch(@"(?=.*delete)(?=.*from)"))
                executeType = ExecuteType.DELETE;

            return executeType;
        }
    }
}
