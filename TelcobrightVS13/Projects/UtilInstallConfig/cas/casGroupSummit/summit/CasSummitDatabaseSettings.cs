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
    public partial class CasSummitAbstractConfigConfigGenerator //quartz config part
    {
        public override DatabaseSetting GetDatabaseSettings()
        {
            var databaseSetting = new DatabaseSetting()
            {
                ServerName = "172.18.0.2",
                BaseDir = @"c:\mysql",
                SocketNameForNamedPipeConnection = "casGroupSummit",
                DatabaseName = this.Tbc.Telcobrightpartner.databasename,
                AdminPassword = "Takay1#$ane",
                AdminUserName = "root",
                DatabaseEngine = "innodb",
                StorageEngineForPartitionedTables = "innodb",
                PartitionStartDate = new DateTime(2023, 1, 1),
                PartitionLenInDays = 1,
                ReadOnlyUserName = "dbreader",
                ReadOnlyPassword = "Takay1takaane"
            };
            Dictionary<string, List<string>> masterConfig = new Dictionary<string, List<string>>()
            {
                {
                    "mysqld", new List<string>
                    {
                        "server-id=1",
                        "log-bin=/var/lib/mysql/master-log-bin",
                        "binlog_format=MIXED",
                        "log-bin-index=/var/lib/mysql/master-bin.index",
                        "binlog-ignore-db=mysql,scheduler",
                        "replicate-ignore-db=mysql,scheduler",
                        "expire-logs-days=7",
                        "thread_stack=512M",
                        "sql_mode=STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION",
                        "default_authentication_plugin=mysql_native_password",
                        "ssl=0",
                        "back_log=50",
                        "datadir=subnetForAppServerNatMasq",
                        "socket= /var/lib/mysql/mysql.sock",
                        "skip-name-resolve=1",
                        "symbolic-links= 0",
                        "lower_case_table_names=1",
                        "open-files-limit=200000",
                        "secure_file_priv=/tmp/",
                        "max_connections=100",
                        "wait_timeout=256",
                        "max_connect_errors=10",
                        "table_open_cache=2048",
                        "max_allowed_packet=512M",
                        "binlog_cache_size=512M",
                        "max_heap_table_size=512M",
                        "read_buffer_size=64M",
                        "read_rnd_buffer_size=64M",
                        "sort_buffer_size=64M",
                        "join_buffer_size=64M",
                        "thread_cache_size=8",
                        "query_cache_size=128M",
                        "query_cache_limit=2M",
                        "ft_min_word_len=4",
                        "default-storage-engine=InnoDB",
                        "transaction_isolation=REPEATABLE-READ",
                        "tmp_table_size=512M",
                        "slow_query_log=0",
                        "slow-query-log-file=/var/log/mysql-slow.log",
                        "long_query_time=2",
                        "tmp-table-size=2G",
                        "max-heap-table-size=2G",
                        "transaction_isolation=REPEATABLE-READ",
                        "tokudb_cache_size=12G",
                        "tokudb_fs_reserve_percent=1",
                        "#malloc-lib=/usr/lib64/libjemalloc.so.1",
                        "log-error=/var/log/mysqld.log",
                        "pid-file=/var/run/mysqld/mysqld.pid",
                        "innodb_buffer_pool_size=1G",
                        "innodb_buffer_pool_instances=8",
                        "innodb_data_file_path=ibdata1:10M:autoextend",
                        "innodb_write_io_threads=8",
                        "innodb_read_io_threads=8",
                        "innodb_thread_concurrency=16",
                        "innodb_flush_log_at_trx_commit=1",
                        "innodb_log_buffer_size=1GB",
                        "innodb_change_buffering=all",
                        "innodb_change_buffer_max_size=25",
                        "innodb_log_file_size=4G",
                        "innodb_log_files_in_group=3",
                        "innodb_max_dirty_pages_pct=90",
                        "innodb_lock_wait_timeout=256",
                    }
                },
                {    "client", new List<string>
                    {
                        "#port=	3306",
                        "#socket=	/var/lib/mysql/mysql.sock",
                        "#user=	root",
                        "#password=	'Takay1#$ane'",
                    }

                },
                {
                    "mysqld_safe", new List<string>
                    {
                        "log-error=	/var/log/mysqld.log",
                        "pid-file=	/var/run/mysqld/mysqld.pid",
                    }

                }
            };

            Dictionary<string, List<string>> slaveConfig = new Dictionary<string, List<string>>()
            {
                {
                    "mysqld", new List<string>
                    {
                        "server-id=2",
                        "#log-bin=/var/lib/mysql/master-log-bin",
                        "#binlog_format=MIXED",
                        "#log-bin-index=/var/lib/mysql/master-bin.index",
                        "binlog-ignore-db=mysql, scheduler",
                        "replicate-ignore-db=mysql, scheduler",
                        "expire-logs-days=7",
                        "thread_stack=512M",
                        "sql_mode=STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION",
                        "default_authentication_plugin=	mysql_native_password",
                        "ssl=0",
                        "back_log=50",
                        "datadir=subnetForAppServerNatMasq",
                        "socket=/var/lib/mysql/mysql.sock",
                        "skip-name-resolve=1",
                        "symbolic-links=0",
                        "lower_case_table_names=1",
                        "open-files-limit=200000",
                        "secure_file_priv=/tmp/",
                        "max_connections=100",
                        "wait_timeout=256",
                        "max_connect_errors=10",
                        "table_open_cache=2048",
                        "max_allowed_packet=512M",
                        "binlog_cache_size=512M",
                        "max_heap_table_size=512M",
                        "read_buffer_size=64M",
                        "read_rnd_buffer_size=64M",
                        "sort_buffer_size=64M",
                        "join_buffer_size=64M",
                        "thread_cache_size=8",
                        "query_cache_size=128M",
                        "query_cache_limit=2M",
                        "ft_min_word_len=4",
                        "default-storage-engine=InnoDB",
                        "transaction_isolation=REPEATABLE-READ",
                        "tmp_table_size=512M",
                        "slow_query_log=0",
                        "slow-query-log-file=/var/log/mysql-slow.log",
                        "long_query_time=2",
                        "tmp-table-size=2G",
                        "max-heap-table-size=2G",
                        "transaction_isolation=REPEATABLE-READ",
                        "tokudb_cache_size=12G",
                        "tokudb_fs_reserve_percent=1",
                        "#malloc-lib=/usr/lib64/libjemalloc.so.1",
                        "log-error=/var/log/mysqld.log",
                        "pid-file=/var/run/mysqld/mysqld.pid",
                        "innodb_buffer_pool_size=1G",
                        "innodb_buffer_pool_instances=8",
                        "innodb_data_file_path=ibdata1:10M:autoextend",
                        "innodb_write_io_threads=8",
                        "innodb_read_io_threads=8",
                        "innodb_thread_concurrency=16",
                        "innodb_flush_log_at_trx_commit=1",
                        "innodb_log_buffer_size=1GB",
                        "innodb_change_buffering=all",
                        "innodb_change_buffer_max_size=25",
                        "innodb_log_file_size=4G",
                        "innodb_log_files_in_group=3",
                        "innodb_max_dirty_pages_pct=90",
                        "innodb_lock_wait_timeout=256"
                    }
                },
                {
                    "client", new List<string>
                    {
                        "#port=3306",
                        "#socket=/var/lib/mysql/mysql.sock",
                        "#user=root",
                        "#password='Takay1#$ane'"
                    }
                },
                {
                    "mysqld_safe", new List<string>
                    {
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
