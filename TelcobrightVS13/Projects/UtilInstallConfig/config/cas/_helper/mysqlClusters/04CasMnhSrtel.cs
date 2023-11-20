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
        public static MySqlCluster getMnhSrtel()
        {
            
            string profileName = "MnhSrtel";

            MySqlCluster MySqlCluster = new MySqlCluster
            {
                Master = new MySqlServer("04MnhSrtel")
                {
                    MySqlVersion = MySqlVersion.MySql57,
                    BindAddressForAutomation = new BindAddress
                    {
                        IpAddressOrHostName = new IpAddressOrHostName { Address = "172.16.1.24" },
                        Port = 3306
                    },
                    RootUserForAutomation = "root",
                    RootPasswordForAutomation = "Takay1takaane$",
                    Users = new List<MySqlUser>()
                    {
                        new MySqlUser(username: CasPortalDbHelper.Db.WriteUserNameForApplication,
                            password: CasPortalDbHelper.Db.WritePasswordForApplication,
                            hostnameOrIpAddresses: AppServerHostnamesForCas.Hostnames,
                            permissions: new List<MySqlPermission>
                            {
                                new MySqlPermission(
                                    new List<MySqlPermissionType> {MySqlPermissionType.all,}, "*"),
                            }),
                        new MySqlUser(username: "dbreader",
                            password: "Takay1takaane",
                            hostnameOrIpAddresses: AppServerHostnamesForCas.Hostnames,
                            permissions:  new List<MySqlPermission>
                            {
                                new MySqlPermission(
                                    new List<MySqlPermissionType>
                                    {
                                        MySqlPermissionType.execute,
                                        MySqlPermissionType.@select
                                    }, "mnh_cas"),

                                new MySqlPermission(
                                    new List<MySqlPermissionType>
                                    {
                                        MySqlPermissionType.execute,
                                        MySqlPermissionType.@select
                                    }, "srtelecom_cas"),
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

