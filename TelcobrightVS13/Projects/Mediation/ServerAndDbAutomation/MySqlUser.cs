using System.Collections.Generic;
using System.Linq;
namespace TelcobrightMediation
{
    public class MySqlUser
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public List<MySqlPermission> Permissions { get; set; }
        public List<string> HostnameOrIpAddresses { get; set; }
        public MySqlUser(string username, string password, List<string> hostnameOrIpAddresses, List<MySqlPermission> permissions)
        {
            Username = username;
            Password = password;
            Permissions = permissions;
            HostnameOrIpAddresses = hostnameOrIpAddresses;
        }
        public string getCreateUsers(string ipAddr)
        {
            return $"CREATE USER IF NOT EXISTS {this.Username}@{ipAddr} IDENTIFIED WITH mysql_native_password BY \"{this.Password}\";";
        }

        public string getAlterUsers(string ipAddr)
        {
            return $"alter user {this.Username}@{ipAddr} identified by \"{this.Password}\";";
        }

        public List<string> getGrantPrivileges(string ipAddr)
        {
            return this.Permissions.Select(p => p.getGrantStatement() + " to " + this.Username + "@" +
                                                ipAddr+ ";").ToList();
        }

        public string getFlushPrivilege()
        {
            return "flush privileges;";
        }

        public List<string> createMySqlUserTelcobrightStyle()
        {
            List<string> lines = new List<string>();
            lines.AddRange(this.HostnameOrIpAddresses.Select(ip=> this.getCreateUsers(ip)));
            lines.AddRange(this.HostnameOrIpAddresses.Select(ip=>this.getAlterUsers(ip)));
            lines.AddRange(this.HostnameOrIpAddresses.SelectMany(ip => this.getGrantPrivileges(ip)));
            lines.Add(this.getFlushPrivilege());
            return lines;
        }
    }
}