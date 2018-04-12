using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using LibraryExtensions;
namespace LibraryExtensions.EntityHelper
{
    public class InformationSchemaRetriever
    {
        List<string> TableNames { get; set; }
        MySqlConnection Con { get; }
        private string DatabaseName { get; }
        public InformationSchemaRetriever(List<string> tableNames, MySqlConnection con,string databaseName)
        {
            this.TableNames = tableNames;
            this.Con = con;
            this.DatabaseName = databaseName;
        }

        public Dictionary<string, List<ischema_columns>>  GetSchemaInformation()
        {
            Dictionary<string, List<ischema_columns>> tableWiseAttributes
                = new Dictionary<string, List<ischema_columns>>();
            List<ischema_columns> lstCols = new List<ischema_columns>();

            Func<string> whereClauseForTableList =
                () =>
                {
                    if (this.TableNames != null && this.TableNames.Count != 0)
                    {
                        return $@" and table_name in({string.Join(",", this.TableNames.Select(c=>c.EncloseWith("'")))})";
                    }
                    return "";
                };

            string sql = $@"SELECT TABLE_NAME,COLUMN_NAME,ORDINAL_POSITION,data_type,is_nullable
                            FROM information_schema.columns
                            WHERE TABLE_SCHEMA = '{this.DatabaseName}'
                            {whereClauseForTableList()}
                            order by ordinal_position";

            using (MySqlCommand cmd = new MySqlCommand(sql, this.Con))
            {
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lstCols.Add(new ischema_columns()
                    {
                        TableNameInDb = reader[0].ToString(),
                        ColumnName = reader[1].ToString(),
                        OrdinalPosition = Convert.ToInt32(reader[2].ToString()),
                        DataType = reader[3].ToString(),
                        IsNullable = (reader[4].ToString().ToLower() == "yes" ? true : false)
                    });
                }
            }
            lstCols.ForEach(e =>
            {
//prepare dictionary
                List<ischema_columns> attrListByTable = null;
                tableWiseAttributes.TryGetValue(e.TableNameInDb, out attrListByTable);
                if (attrListByTable == null)
                {
                    attrListByTable = new List<ischema_columns>();
                    tableWiseAttributes.Add(e.TableNameInDb, attrListByTable);
                }
                attrListByTable.Add(e);
            });
            return tableWiseAttributes;
        }
    }
}