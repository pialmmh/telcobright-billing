using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation.ServerAndDbAutomation
{
    public class MySqlShellAutomationHelper
    {
        public static List<string> createOrAlterUserLinux(List<MySqlUser> users)
        {
            List<string> shellCommands= new List<string>();
            foreach (MySqlUser user in users)
            {
                List<string> ipAddresses = user.HostnameOrIpAddresses;
                Func<string, List<string>> getHostScript = ipAddressOrHost =>
                {
                    string userName = user.Username;
                    string password = user.Password;
                    return new List<string>
                    {
                        $"sudo mysql -uroot -e 'CREATE USER IF NOT EXISTS {userName}@{ipAddressOrHost} IDENTIFIED WITH mysql_native_password BY \"{password}\";'",
                        $"sudo mysql -uroot -e 'alter user {userName}@{ipAddressOrHost} identified by \"{password}\";'",
                        $"sudo mysql -uroot -e '{user.Permissions.Select(p=>p.getGrantStatement() + " to " + userName+"@"+ipAddressOrHost)+";"}'",
                        $"sudo mysql -uroot -e 'flush privileges;'"
                    };
                };
                foreach (string ipAddress in ipAddresses)
                {
                    List<string> commands = getHostScript(ipAddress);
                    shellCommands.AddRange(commands);
                }
            }
            return shellCommands;
        }
        public static List<string> createOrAlterUserWindows(List<MySqlUser> users)
        {
            List<string> shellCommands = new List<string>();
            foreach (MySqlUser user in users)
            {
                List<string> ipAddresses = user.HostnameOrIpAddresses;
                Func<string, List<string>> getHostScript = ipAddressOrHost =>
                {
                    string userName = user.Username;
                    string password = user.Password;
                    return new List<string>
                    {
                        $"mysql -uroot -e 'CREATE USER IF NOT EXISTS {userName}@{ipAddressOrHost} IDENTIFIED WITH mysql_native_password BY \"{password}\";'",
                        $"mysql -uroot -e 'alter user {userName}@{ipAddressOrHost} identified by \"{password}\";'",
                        $"mysql -uroot -e '{user.Permissions.Select(p=>p.getGrantStatement())} to {userName}@{ipAddressOrHost};'",
                        $"mysql -uroot -e 'flush privileges;'"
                    };
                };
                foreach (string ipAddress in ipAddresses)
                {
                    List<string> commands = getHostScript(ipAddress);
                    shellCommands.AddRange(commands);
                }
            }
            return shellCommands;
        }
    }
}
