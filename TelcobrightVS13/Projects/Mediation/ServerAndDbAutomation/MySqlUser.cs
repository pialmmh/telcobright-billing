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
        public string getCreateUser()
        {
            return $"CREATE USER IF NOT EXISTS {this.Username}@{this.HostnameOrIpAddresses} IDENTIFIED WITH mysql_native_password BY \"{this.Password}\";";
        }

        public string getAlterUser()
        {
            return $"alter user {this.Username}@{this.HostnameOrIpAddresses} identified by \"{this.Password}\";";
        }

        public List<string> getGrantPrivileges()
        {
            return this.Permissions.Select(p => p.getGrantStatement() + " to " + this.Username + "@" +
                                                this.HostnameOrIpAddresses + ";").ToList();
        }

        public string getFlushPrivilege()
        {
            return "flush privileges;";
        }

        public List<string> createMySqlUserInTelcobright()
        {
            List<string> lines = new List<string>
            {
                this.getCreateUser(),
                this.getAlterUser(),
            };
            List<string> grantLines = this.getGrantPrivileges();
            lines.AddRange(grantLines);
            lines.Add(this.getFlushPrivilege());
            return lines;
        }
    }
}