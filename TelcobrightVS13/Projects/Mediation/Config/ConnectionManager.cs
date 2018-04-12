using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using LibraryExtensions.ConfigHelper;

namespace TelcobrightMediation.Config
{
    public static class ConnectionManager
    {
        public static string GetConnectionStringByOperator(string operatorShortName)
        {
            return ConfigurationManager.ConnectionStrings["Partner"].ConnectionString
                .Replace("#DatabaseName#", operatorShortName);
        }
        public static string GetEntityConnectionStringByOperator(string operatorShortName)
        {
            return ConfigurationManager.ConnectionStrings["PartnerEntities"].ConnectionString
                .Replace("#DatabaseName#", operatorShortName);
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
