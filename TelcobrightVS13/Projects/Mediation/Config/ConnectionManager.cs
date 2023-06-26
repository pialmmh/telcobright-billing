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
        static public DatabaseSetting databasetting { get; set; }
        public static string GetConnectionStringByOperator(string operatorShortName, TelcobrightConfig tbc = null)
        {
            CacheConnectionStrings(operatorShortName, tbc);
            return ConfigurationManager.ConnectionStrings["Partner"].ConnectionString
                .Replace("#DatabaseName#", operatorShortName);
        }
        public static string GetEntityConnectionStringByOperator(string operatorShortName, TelcobrightConfig tbc)
        {
            string constr = GetEntityConnectionStringByType("PartnerEntities", operatorShortName, tbc);
            return constr;
        }
        public static string GetEntityConnectionStringByType(string connectionType, string operatorShortName, TelcobrightConfig tbc = null)
        {
            CacheConnectionStrings(operatorShortName, tbc);

            DatabaseSetting dbSettings = databasetting;
            //if (dbSettings.OverrideDatabaseSettingsFromAppConfig == false)
            //{
            //    return ConfigurationManager.ConnectionStrings[$"{connectionType}"].ConnectionString
            //            .Replace("#DatabaseName#", operatorShortName);
            //}
            string constr = "";
            switch (connectionType)
            {
                case "Partner":

                    break;
                case "PartnerEntities":
                    constr = $"metadata=res://*/PartnerModel.csdl|res://*/PartnerModel.ssdl|res://*/PartnerModel.msl;provider=MySql.Data.MySqlClient;provider connection string=\"server = {dbSettings.ServerName}; user id = {dbSettings.AdminUserName}; password = {dbSettings.AdminPassword};persistsecurityinfo=True;Convert Zero Datetime=True;default command timeout=300;database={dbSettings.DatabaseName}\"";
                    break;
            }

            return constr;
        }

        public static string GetEntityConnectionString(DatabaseSetting databaseSetting)
        {
            return $"metadata=res://*/PartnerModel.csdl|res://*/PartnerModel.ssdl|res://*/PartnerModel.msl;" +
                   $"provider=MySql.Data.MySqlClient;" +
                   $"provider connection string=\"server = {databaseSetting.ServerName}; user id = {databaseSetting.AdminUserName}; password = {databaseSetting.AdminPassword};persistsecurityinfo=True;Convert Zero Datetime=True;default command timeout=300;database={databaseSetting.DatabaseName}\"";
        }

        private static void CacheConnectionStrings(string operatorShortName, TelcobrightConfig tbc)
        {
            if (tbc != null)
            {
                databasetting=tbc.DatabaseSetting;
            }
        }

        public static DbCommand CreateCommandFromDbContext(DbContext context)
        {
            DbCommand command = context.Database.Connection.CreateCommand();
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
        public static DbCommand CreateCommandFromDbContext(DbContext context, CommandType commandType, string commandText)
        {
            DbCommand command = context.Database.Connection.CreateCommand();
            command.CommandType = commandType;
            command.CommandText = commandText;
            return command;
        }
    }
}
