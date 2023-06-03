using System;
using System.Web.Script.Serialization;
using System.Linq;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using LibraryExtensions;
namespace TelcobrightMediation
{
    public enum ServerOs
    {
        Ubuntu,
        Win2012
    }

    public class ServerInstance
    {
        private List<IpAddress> ipAddresses;

        public int ServerId { get; set; }
        public ServerOs ServerOs { get; set; }
        public List<IpAddress> IpAddresses { get; set; }
        public string AutomationUsername { get; set; }
        public string AutomationPassword { get; set; }

        public ServerInstance(int serverId, ServerOs serverOs, List<IpAddress> ipAddresses)
        {
            this.ServerId = serverId;
            this.ServerOs = serverOs;
            this.IpAddresses = ipAddresses;

        }

        public ServerInstance(int serverId, List<IpAddress> ipAddresses)
        {
            ServerId = serverId;
            this.ipAddresses = ipAddresses;
        }
    }

    public class MySqlUser
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public List<string> Permissions { get; set; }
        public List<string> HostnameOrIPAddresses { get; set; }
        public MySqlUser(string username, string password, List<string> permissions, List<string> hostnameOrIpAddresses)
        {
            Username = username;
            Password = password;
            Permissions = permissions;
            HostnameOrIPAddresses = hostnameOrIpAddresses;
        }
    }

    public class ReplicationSlaveInstance
    {
        public string MasterHostnameOrIp { get; set; }
        public int MasterPort { get; set; }
        public string MasterUsername { get; set; }
        public string MasterPassword { get; set; }
        public int RetryCount { get; set; } = 10;

        public void startReplication(MySqlConnection con)
        {
            string masterLogFileName;
            long logPosition;
            readMasterLogPosition(con, out masterLogFileName,out logPosition);
            string commandText = $@"CHANGE MASTER TO MASTER_HOST='{MasterHostnameOrIp}',
                                    MASTER_USER='{MasterUsername}',
                                    MASTER_PASSWORD='{MasterPassword}',
                                    MASTER_PORT={MasterPort},
                                    MASTER_LOG_FILE='{masterLogFileName}',
                                    MASTER_LOG_POS={logPosition},
                                    MASTER_CONNECT_RETRY={RetryCount};";
            MySqlCommand cmd = new MySqlCommand();
            cmd.ExecuteCommandText(commandText);
        }

        void readMasterLogPosition(MySqlConnection con,out string logFileName, out long position)
        {
            string masterStatus = DbUtil.execCommandAndGetOutput(con, @"show master status \G");
            string[] lines = masterStatus.Split(
                new string[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
            );
            logFileName=lines.First(l => l.Contains("File")).Split(':')[1].Trim();
            position=Convert.ToInt64(lines.First(l => l.Contains("Position")).Split(':')[1].Trim());
        }
    }

    public class MySqlInstance
    {
        public string AutomationType { get; set; }
        public Dictionary<string, string> ConfigParams { get; set; }
        public List<MySqlUser> Users { get; set; }
        public List<string> IgnoreDatabasesFromReplication { get; set; }=new List<string>() {"mysql"};
        public List<string> ReplicateDbsFromMaster { get; set; }
        public List<MySqlInstance> SlaveInstances { get; set; }
        

        public MySqlInstance(string automationType, Dictionary<string, string> configParams)
        {
            AutomationType = automationType;
            ConfigParams = configParams;
        }
    }

}
