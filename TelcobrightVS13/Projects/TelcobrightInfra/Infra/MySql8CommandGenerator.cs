using System.Collections.Generic;
using System.Linq;

namespace TelcobrightInfra
{
    public class MySql8CommandGenerator : MySqlCommandGenerator
    {
        public override string getCreateUsers(MySqlUser user, string ipAddr)
        {
            return $"CREATE USER IF NOT EXISTS {user.Username}@{ipAddr} IDENTIFIED WITH mysql_native_password BY \"{user.Password}\";";
        }
        public override string getAlterUsers(MySqlUser user, string ipAddr)
        {
            return $"alter user {user.Username}@{ipAddr} identified by \"{user.Password}\";";
        }
        public override List<string> getGrantPrivileges(MySqlUser user, string ipAddr)
        {
            return user.Permissions.Select(p => p.getGrantStatement() + " to " + user.Username + "@" +
                                                ipAddr + ";").ToList();
        }
        public override string getFlushPrivilege()
        {
            return "flush privileges;";
        }
    }
}