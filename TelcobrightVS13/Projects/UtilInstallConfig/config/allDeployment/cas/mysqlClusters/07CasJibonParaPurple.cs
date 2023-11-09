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
        public static MySqlCluster getJibonParaPurple()
        {
            
            string profileName = "JibonParaPurple";

            MySqlCluster MySqlCluster = new MySqlCluster
            {
                Master = new MySqlServer("07JibonParaPurple")
                {
                    MySqlVersion = MySqlVersion.MySql57,
                    BindAddressForAutomation = new BindAddress
                    {
                        IpAddressOrHostName = new IpAddressOrHostName { Address = "10.100.150.26" },
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
                                    }, "jibondhara_cas"),

                                new MySqlPermission(
                                    new List<MySqlPermissionType>
                                    {
                                        MySqlPermissionType.execute,
                                        MySqlPermissionType.@select
                                    }, "purple_cas"),
                                new MySqlPermission(
                                    new List<MySqlPermissionType>
                                    {
                                        MySqlPermissionType.execute,
                                        MySqlPermissionType.@select
                                    }, "paradise_cas"),
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

