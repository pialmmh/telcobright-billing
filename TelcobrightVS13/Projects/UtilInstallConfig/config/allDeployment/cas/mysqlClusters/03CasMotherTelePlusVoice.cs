using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstallConfig.config.allDeployment.cas.mysqlClusters;
using InstallConfig._generator;
using TelcobrightInfra;

namespace InstallConfig
{
    public static partial class AllDeploymenProfiles
    {
        public static MySqlCluster getMotherTelePlusVoice()
        {
            
            string profileName = "MotherTelePlusVoice";
            MySqlCluster MySqlCluster = new MySqlCluster
            {
                Master = new MySqlServer("03MotherTelePlusVoice")
                {
                    MySqlVersion = MySqlVersion.MySql57,
                    BindAddressForAutomation = new BindAddress
                    {
                        IpAddressOrHostName = new IpAddressOrHostName { Address = "10.100.150.22" },
                        Port = 3306
                    },
                    RootUserForAutomation = "root",
                    RootPasswordForAutomation = "Takay1takaane$",
                    Users = new List<MySqlUser>()
                    {
                        new MySqlUser(username: CasDbHelperOld.Db.AdminUserName,
                            password: CasDbHelperOld.Db.AdminPassword,
                            hostnameOrIpAddresses: AppServerHostnamesForCas.Hostnames,
                            permissions: new List<MySqlPermission>
                            {
                                new MySqlPermission(
                                    new List<MySqlPermissionType> {MySqlPermissionType.all,}, "*"),
                            }),
                        new MySqlUser(username: "dbreader",
                            password: "Takay1takaane",
                            hostnameOrIpAddresses: AppServerHostnamesForCas.Hostnames,
                            permissions: new List<MySqlPermission>
                            {
                                new MySqlPermission(
                                    new List<MySqlPermissionType>
                                    {
                                        MySqlPermissionType.execute,
                                        MySqlPermissionType.@select
                                    }, "mothertelecom_cas"),

                                new MySqlPermission(
                                    new List<MySqlPermissionType>
                                    {
                                        MySqlPermissionType.execute,
                                        MySqlPermissionType.@select
                                    }, "teleplusnewyork_cas"),
                                new MySqlPermission(
                                    new List<MySqlPermissionType>
                                    {
                                        MySqlPermissionType.execute,
                                        MySqlPermissionType.@select
                                    }, "voicetel_cas"),
                                new MySqlPermission(
                                    new List<MySqlPermissionType>
                                    {
                                        MySqlPermissionType.execute,
                                        MySqlPermissionType.@select
                                    }, "btrc_cas")
                            })
                    }
                },

                Slaves = new List<MySqlServer>()
                {
                    CommonSlaveWin01.Win01
                }
            };

            return MySqlCluster;

        }
    }
}

