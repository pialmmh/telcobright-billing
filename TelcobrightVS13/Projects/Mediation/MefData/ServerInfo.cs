using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation
{
    public class ServerInfo
    {
        ServerAccessProtocol ServerAccessProtocol { get; set; }
        public string ServerNameOrIp { get; set; }
        public string  Username { get; set; }
        public string Password { get; set; }
        public ServerInfo(string serverNameOrIp, string username, string password) {
            this.ServerNameOrIp = serverNameOrIp;
            this.Username = username;
            this.Password = password;
        }
    }
}
