using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstallConfig.config.allDeployment.cas.mysqlClusters;
using TelcobrightInfra;

namespace InstallConfig
{
    public class CommonSlaveWin01//103.98.66.5
    {
        public  static MySqlServer Win01 = new MySqlServer("CAS-103.98.66.5")
        {
            MySqlVersion = MySqlVersion.percona57,
            BindAddressForAutomation = new BindAddress
            {
                IpAddressOrHostName = new IpAddressOrHostName {Address = "103.98.66.5"}, //Container IP
                Port = 3306
            },
            RootUserForAutomation = "btrc",
            RootPasswordForAutomation = "Takay1takaane",
            Users = new List<MySqlUser>()
            {
                new MySqlUser(username: CasNewDbHelper.Db.AdminUserName,
                    password: CasNewDbHelper.Db.AdminPassword,
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
                            }, "agni_cas"),

                        new MySqlPermission(
                            new List<MySqlPermissionType>
                            {
                                MySqlPermissionType.execute,
                                MySqlPermissionType.@select
                            }, "banglatelecom_cas"),
                        new MySqlPermission(
                            new List<MySqlPermissionType>
                            {
                                MySqlPermissionType.execute,
                                MySqlPermissionType.@select
                            }, "bantel_cas"),
                        new MySqlPermission(
                            new List<MySqlPermissionType>
                            {
                                MySqlPermissionType.execute,
                                MySqlPermissionType.@select
                            }, "btrc_cas")
                    })
            }
        };
    }
}
