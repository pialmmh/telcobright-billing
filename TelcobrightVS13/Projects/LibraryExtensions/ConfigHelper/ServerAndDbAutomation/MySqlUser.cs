using System.Collections.Generic;

namespace LibraryExtensions.ConfigHelper
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
    }
}