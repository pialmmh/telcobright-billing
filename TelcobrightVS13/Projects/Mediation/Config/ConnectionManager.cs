using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using LibraryExtensions.ConfigHelper;
using System.Linq;
using System.Collections.Generic;
namespace TelcobrightMediation.Config
{
    public static class ConnectionManager
    {
        static Dictionary<string, DatabaseSetting> operatorWiseDatabasettings { get; set; } = new Dictionary<string, DatabaseSetting>();
        public static string GetConnectionStringByOperator(string operatorShortName, TelcobrightConfig tbc = null)
        {
            CacheConnectionStrings(operatorShortName, tbc);
            return ConfigurationManager.ConnectionStrings["Partner"].ConnectionString
                .Replace("#DatabaseName#", operatorShortName);
        }
        public static string GetEntityConnectionStringByOperator(string operatorShortName, TelcobrightConfig tbc = null)
        {
            string constr = GetEntityConnectionStringByType("PartnerEntities", operatorShortName, tbc);
            return constr;
        }
        public static string GetEntityConnectionStringByType(string connectionType,string operatorShortName, TelcobrightConfig tbc = null)
        {
            CacheConnectionStrings(operatorShortName, tbc);

            DatabaseSetting dbSettings = null;
            operatorWiseDatabasettings.TryGetValue(operatorShortName, out dbSettings);
            if (dbSettings.OverrideDatabaseSettingsFromAppConfig == false)
            {
                return ConfigurationManager.ConnectionStrings[$"{connectionType}"].ConnectionString
                        .Replace("#DatabaseName#", operatorShortName);
            }
            string constr = "";
            switch (connectionType) {
                case "Partner":

                    break;
                case "PartnerEntities":
                    constr = $"metadata=res://*/PartnerModel.csdl|res://*/PartnerModel.ssdl|res://*/PartnerModel.msl;provider=MySql.Data.MySqlClient;provider connection string=\"server = {dbSettings.ServerName}; user id = {dbSettings.AdminUserName}; password = {dbSettings.AdminPassword};persistsecurityinfo=True;Convert Zero Datetime=True;default command timeout=300;database=btel2\"";
                    break;
            }

            return constr;
        }

        private static void CacheConnectionStrings(string operatorShortName, TelcobrightConfig tbc)
        {
            if (tbc != null)
            {
                if (!operatorWiseDatabasettings.ContainsKey(operatorShortName))
                {
                    operatorWiseDatabasettings.Add(operatorShortName, tbc.DatabaseSetting);
                }
            }
        }

        public static DbCommand CreateCommandFromDbContext(DbContext context)
        {
            DbCommand command=context.Database.Connection.CreateCommand();
            command.CommandType = CommandType.Text;
            return command;
        }
        public static DbCommand CreateCommandFromDbContext(DbContext context, string commandText)
        {
            DbCommand command = context.Database.Connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = commandText;
            return command;
        }
        public static DbCommand CreateCommandFromDbContext(DbContext context,CommandType commandType, string commandText)
        {
            DbCommand command = context.Database.Connection.CreateCommand();
            command.CommandType = commandType;
            command.CommandText = commandText;
            return command;
        }
    }
}
