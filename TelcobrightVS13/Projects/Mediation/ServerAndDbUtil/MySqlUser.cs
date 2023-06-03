using System.Collections.Generic;

namespace TelcobrightMediation
{
    public class MySqlUser
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public List<MySqlPermission> Permisions { get; set; }
        public List<string> HostnameOrIpAddresses { get; set; }
        public MySqlUser(string username, string password, List<MySqlPermission> permisions, List<string> hostnameOrIpAddresses)
        {
            Username = username;
            Password = password;
            Permisions = permisions;
            HostnameOrIpAddresses = hostnameOrIpAddresses;
        }
    }
}