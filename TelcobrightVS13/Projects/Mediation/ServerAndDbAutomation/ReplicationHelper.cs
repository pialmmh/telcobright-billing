using System;
using System.Linq;
using MySql.Data.MySqlClient;

namespace TelcobrightMediation
{
    public class ReplicationHelper
    {
        public int RetryCount { get; set; } = 10;
        public void startReplication(MySqlConnection con)
        {
            string masterLogFileName;
            long logPosition;
            readMasterLogPosition(con, out masterLogFileName, out logPosition);
            /*string commandText = $@"CHANGE MASTER TO MASTER_HOST='{MasterHostnameOrIp}',
                                    MASTER_USER='{MasterUsername}',
                                    MASTER_PASSWORD='{MasterPassword}',
                                    MASTER_PORT={MasterPort},
                                    MASTER_LOG_FILE='{masterLogFileName}',
                                    MASTER_LOG_POS={logPosition},
                                    MASTER_CONNECT_RETRY={RetryCount};";
            MySqlCommand cmd = new MySqlCommand();
            cmd.ExecuteCommandText(commandText);*/
        }

        void readMasterLogPosition(MySqlConnection con, out string logFileName, out long position)
        {
            string masterStatus = DbUtil.execCommandAndGetOutput(con, @"show master status \G");
            string[] lines = masterStatus.Split(
                new string[] {"\r\n", "\r", "\n"},
                StringSplitOptions.None
            );
            logFileName = lines.First(l => l.Contains("File")).Split(':')[1].Trim();
            position = Convert.ToInt64(lines.First(l => l.Contains("Position")).Split(':')[1].Trim());
        }
    }
}