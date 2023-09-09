using System.Collections.Generic;
using System.Linq;

namespace TelcobrightInfra
{
    public class MySqlCommandGenerator
    {
        public virtual string getCreateUsers(MySqlUser user, string ipAddr)
        {
            return "CREATE USER IF NOT EXISTS " + user.Username + "@" +ipAddr + " IDENTIFIED WITH mysql_native_password BY '{"+ user.Password + "}';";
        }
        public virtual string getAlterUsers(MySqlUser user, string ipAddr)
        {
            return $"alter user {user.Username}@{ipAddr} identified by '"+ user.Password + "';";
        }
        public virtual List<string> getGrantPrivileges(MySqlUser user, string ipAddr)
        {
            return user.Permissions.Select(p => p.getGrantStatement() + " to " + user.Username + "@" +
                                                ipAddr + ";").ToList();
        }
        public virtual string getFlushPrivilege()
        {
            return "flush privileges;";
        }
        public virtual List<string> createMySqlUserTelcobrightStyle(MySqlUser user)
        {
            List<string> lines = new List<string>();
            lines.AddRange(user.HostnameOrIpAddresses.Select(ip => this.getCreateUsers(user, ip)));
            lines.AddRange(user.HostnameOrIpAddresses.Select(ip => this.getAlterUsers(user, ip)));
            lines.AddRange(user.HostnameOrIpAddresses.SelectMany(ip => this.getGrantPrivileges(user, ip)));
            lines.Add(this.getFlushPrivilege());
            return lines;
        }
    }
}