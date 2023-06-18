using System.Collections.Generic;
using System.Linq;

namespace LibraryExtensions.ConfigHelper
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

        public MySqlPermission(List<MySqlPermissionType> permissionTypes, string databaseName)
        {
            PermissionTypes = permissionTypes;
            DatabaseName = databaseName;
        }

        public string getGrantStatement()
        {
            return $"grant {string.Join(",", this.PermissionTypes.Select(p => p.ToString()))} on {DatabaseName}";
        }
    }
}