using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation
{
    public enum ServerAccessProtocol{
        SSHV2, 
        Telnet
    }
    public class ServerCredential
    {
        ServerAccessProtocol ServerAccessProtocol { get; set; }
        public string ServerNameOrIp { get; set; }
        public string  Username { get; set; }
        public string Password { get; set; }
        public ServerCredential(string serverNameOrIp, string username, string password) {
            this.ServerNameOrIp = serverNameOrIp;
            this.Username = username;
            this.Password = password;
        }
    }
}
