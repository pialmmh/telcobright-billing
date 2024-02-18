using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using TelcobrightMediation;
using TelcobrightMediation.Scheduler.Quartz;
using System.ComponentModel.Composition;
using LibraryExtensions;
using LibraryExtensions.ConfigHelper;
using QuartzTelcobright;
using TelcobrightFileOperations;
using TelcobrightMediation.Automation;
using TelcobrightMediation.Config;


namespace InstallConfig
{
    public partial class MirAbstractConfigGenerator //quartz config part
    {
        public override DatabaseSetting GetDatabaseConfigs()
        {
            var databaseSetting = new DatabaseSetting()
            {
                ServerName = "192.168.25.236",
                DatabaseName = this.Tbc.Telcobrightpartner.databasename,
                WritePasswordForApplication = "Takay1#$ane",
                WriteUserNameForApplication = "root",
                DatabaseEngine = "innodb",
                StorageEngineForPartitionedTables = "tokudb",
                PartitionStartDate = new DateTime(2023, 1, 1),
                PartitionLenInDays = 1,
                ReadOnlyUserNameForApplication = "dbreader",
                ReadOnlyPasswordForApplication = "Takay1takaane"
            };
            Dictionary<string, List<string>> masterConfig = new Dictionary<string, List<string>>()
            {
                {
                    "mysqld", new List<string>
                    {
                        "skip-name-resolve",
                        "datadir=/telcobright/mysql",
                        "socket=/var/lib/mysql/mysql.sock",
                        "user=mysql",
                        "symbolic-links=0",
                        "character-set-server=utf8",
                        "collation-server=utf8_general_ci",
                        "lower_case_table_names=1",
                        "open-files-limit = 1000000",
                        "user                           = mysql",
                        "default-storage-engine         = InnoDB",
                        "server-id=1",
                        "binlog_format=MIXED",
                        "log-bin                        = /telcobright/mysql/master-bin",
                        "log-bin-index=/telcobright/mysql/master-bin.index",
                        "expire-logs-days               = 3",
                        "sync-binlog                    = 1",
                        "secure_file_priv=/tmp/",
                        "max_allowed_packet =500M",
                        "max-connect-errors             = 1000000",
                        "tmp-table-size                 = 4G",
                        "max-heap-table-size            = 8G",
                        "query-cache-type               = 1",
                        "query_cache_size = 2000M",
                        "max-connections                = 50",
                        "thread-cache-size              = 8",
                        "open-files-limit               = 65535",
                        "table-definition-cache         = 1024",
                        "table-open-cache               = 2048",
                        "innodb-flush-method            = O_DIRECT",
                        "innodb-log-files-in-group      = 2",
                        "innodb-log-file-size           = 512M",
                        "innodb-flush-log-at-trx-commit = 1",
                        "innodb-file-per-table          = 1",
                        "innodb-buffer-pool-size        = 3G",
                        "tokudb_cache_size=6G",
                        "log-error                      = /telcobright/mysql/mysql-error.log",
                        "log-queries-not-using-indexes  = 1",
                        "slow-query-log                 = 0",
                        "slow-query-log-file            = /telcobright/mysql/mysql-slow.log",
                    }
                },
                {
                    "mysqld_safe", new List<string>
                    {
                        "thp-setting=never",
                        "malloc-lib= /usr/lib64/libjemalloc.so.1",
                        "log-error=/var/log/mysqld.log",
                        "pid-file=/var/run/mysqld/mysqld.pid",
                    }

                }
            };

            Dictionary<string, List<string>> slaveConfig = new Dictionary<string, List<string>>()
            {
                {
                    "mysqld", new List<string>
                    {

                        "tokudb_cache_size=4G",
                        "skip-name-resolve",
                        "datadir=/telcobright/mysql",
                        "socket=/var/lib/mysql/mysql.sock",
                        "user=mysql",
                        "symbolic-links=0",
                        "character-set-server=utf8",
                        "collation-server=utf8_general_ci",
                        "lower_case_table_names=1",
                        "open-files-limit = 1000000",
                        "user                           = mysql",
                        "default-storage-engine         = InnoDB",
                        "server-id=2",
                        "binlog_format=MIXED",
                        "log-bin                        = /telcobright/mysql/slave-relay-bin",
                        "log-bin-index=/telcobright/mysql/slave-relay-bin.index",
                        "expire-logs-days               = 3",
                        "sync-binlog                    = 1",
                        "relay-log-index= slave-relay-bin.index",
                        "relay-log= slave-relay-bin",
                        "log-error = /telcobright/mysql/mysql-error.log",
                        "#tokudb_cache_size= 12G",
                        "max_allowed_packet = 500M",
                        "max-connect-errors             = 1000000",
                        "tmp-table-size                 = 1G",
                        "max-heap-table-size            = 2G",
                        "query-cache-type               = 1",
                        "query_cache_size = 2000M",
                        "max-connections                = 50",
                        "thread-cache-size              = 2000M",
                        "open-files-limit               = 65535",
                        "table-definition-cache         = 1024",
                        "table-open-cache               = 2048",
                        "innodb-flush-method            = O_DIRECT",
                        "innodb-log-files-in-group      = 2",
                        "innodb-log-file-size           = 512M",
                        "innodb-flush-log-at-trx-commit = 1",
                        "innodb-file-per-table          = 1",
                        "innodb-buffer-pool-size        = 1000M",
                        "log-error                      = /telcobright/mysql/mysql-error.log",
                        "log-queries-not-using-indexes  = 1",
                        "slow-query-log                 = 0",
                        "slow-query-log-file            = /telcobright/mysql/mysql-slow.log",
                    }
                },
                {
                    "mysqld_safe", new List<string>
                    {
                        "thp-setting=never",
                        "malloc-lib= /usr/lib64/libjemalloc.so.1",
                        "log-error=/var/log/mysqld.log",
                        "pid-file=/var/run/mysqld/mysqld.pid"
                    }
                }

            };
            //MySqlServer mySqlServer= new MySqlServer();


            return databaseSetting;
        }
    }
}