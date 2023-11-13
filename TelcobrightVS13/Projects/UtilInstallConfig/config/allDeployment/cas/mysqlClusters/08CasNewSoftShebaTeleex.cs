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
        public static MySqlCluster getNewSoftShebaTeleex()
        {
            
            string profileName = "NewSoftShebaTeleex";

            MySqlCluster MySqlCluster = new MySqlCluster
            {
                Master = new MySqlServer("08NewSoftShebaTeleex")
                {
                    MySqlVersion = MySqlVersion.MySql57,
                    BindAddressForAutomation = new BindAddress
                    {
                        IpAddressOrHostName = new IpAddressOrHostName { Address = "10.100.150.27" },
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
                            permissions: new List<MySqlPermission>
                            {
                                new MySqlPermission(
                                    new List<MySqlPermissionType>
                                    {
                                        MySqlPermissionType.execute,
                                        MySqlPermissionType.@select
                                    }, "teleexchange_cas"),

                                new MySqlPermission(
                                    new List<MySqlPermissionType>
                                    {
                                        MySqlPermissionType.execute,
                                        MySqlPermissionType.@select
                                    }, "softex_cas"),
                                new MySqlPermission(
                                    new List<MySqlPermissionType>
                                    {
                                        MySqlPermissionType.execute,
                                        MySqlPermissionType.@select
                                    }, "sheba_cas"),
                                new MySqlPermission(
                                    new List<MySqlPermissionType>
                                    {
                                        MySqlPermissionType.execute,
                                        MySqlPermissionType.@select
                                    }, "newgenerationtelecom_cas"),
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

