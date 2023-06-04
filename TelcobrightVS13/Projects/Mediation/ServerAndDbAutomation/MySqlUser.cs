using System.Collections.Generic;

namespace TelcobrightMediation
{
    public class MySqlUser
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public List<MySqlPermission> Permissions { get; set; }
        public List<string> HostnameOrIpAddresses { get; set; }
        public MySqlUser(string username, string password, List<MySqlPermission> permissions, List<string> hostnameOrIpAddresses)
        {
            Username = username;
            Password = password;
            Permissions = permissions;
            HostnameOrIpAddresses = hostnameOrIpAddresses;
        }
    }
}