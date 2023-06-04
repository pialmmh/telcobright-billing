using System.Collections.Generic;
using System.Linq;

namespace TelcobrightMediation
{
    public enum MySqlPermissionType
    {
        all,
        select,
        execute,
        replication
    }

    public class MySqlPermission
    {
        public List<MySqlPermissionType> PermissionTypes { get; set; }
        public string DatabaseName { get; set; }

        public string getGrantStatement()
        {
            return $"grant {string.Join(",", this.PermissionTypes.Select(p => p.ToString()))} on {DatabaseName}";
        }
    }
}