using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;
using LibraryExtensions;
using System.IO;
using Utils.Models;

namespace Utils
{
    class ClassDeclaration
    {
        public string tableName { get; }
        private List<string> columns { get; }
        public bool partial { get; }
        private Func<int, string> GetLineSeparator { get; }
        public ClassDeclaration(string tableName, List<string> columns, bool partial)
        {
            this.tableName = tableName;
            this.columns = columns;
            this.partial = partial;
            GetLineSeparator = cIndent => Environment.NewLine + StringBuilderExtensions.GettabStr(cIndent);
        }
        public string GetExtInsertColumns()
        {
            //put everything in one line
            var propertyGet=new StringBuilder().Append($@" insert into ").Append(this.tableName).Append("(")
                .Append(string.Join(",", columns)).Append(") values ").ToString();
            return new StringBuilder("\t\tpublic static string ")
                .Append(this.tableName).Append(" { get { return ").Append("\"")
                .Append(propertyGet)
                .Append("\"")
                .Append(";} }").ToString();
        }
        void GetExtInsertValues(StringBuilder sb)
        {
            IEnumerable<string> valuesToMySqlFields = columns.Select(c => new StringBuilder("{").Append(c).Append(".ToMySqlField()}").ToString());
            int startIndent = 2;
            sb.AppendLineWithIndent("public string GetExtInsertValues()", startIndent)
                .AppendLineWithIndent("{", startIndent)
                .AppendLineWithIndent("return $@\"(", 3)
                .AppendLineWithIndent(string.Join("," + GetLineSeparator(4), valuesToMySqlFields), 4)
                .AppendLineWithIndent(")\";", 4)
                .AppendLineWithIndent("}", startIndent);
        }
        void GetExtInsertCustom(StringBuilder sb)
        {
            int startIndent = 2;
            sb.AppendWithIndent("public string GetExtInsertCustom(Func<", startIndent).Append(tableName).Append(",string> externalInsertMethod").AppendLine(")")
                .AppendLineWithIndent("{", startIndent)
                .AppendLineWithIndent("return externalInsertMethod.Invoke(this);", 3)
                .AppendLineWithIndent("}", startIndent);
        }
        void GetUpdateCommands(StringBuilder sb)
        {
            IEnumerable<string> updateLines = columns.Select(c => (new StringBuilder(c).Append("=").Append("{" + c + ".ToMySqlField()+\" \"" + "}"))
                                                                  .ToString());
            int startIndent = 2;
            sb.AppendWithIndent("public string GetUpdateCommand(Func<", startIndent).Append(tableName).AppendLine(",string> whereClauseMethod)")
                .AppendLineWithIndent("{", startIndent)
                .AppendWithIndent("return $@\"update ", 3).Append(this.tableName).AppendLine(" set ")
                .AppendLineWithIndent(string.Join("," + GetLineSeparator(4), updateLines), 4)
                .AppendWithIndent("{whereClauseMethod.Invoke(this)}", 4).AppendLine(";")
                .AppendLineWithIndent("\";", 4)
                .AppendLineWithIndent("}", startIndent);
        }
        void GetUpdateCommandsCustom(StringBuilder sb)
        {
            IEnumerable<string> updateLines = columns.Select(c => (new StringBuilder(c).Append("=").Append("{" + c + ".ToMySqlField()+\" \"" + "}"))
                                                                  .ToString());
            int startIndent = 2;
            sb.AppendWithIndent("public string GetUpdateCommandCustom(Func<", startIndent).Append(tableName).AppendLine(",string> updateCommandMethodCustom)")
            .AppendLineWithIndent("{", startIndent)
                .AppendWithIndent("return updateCommandMethodCustom.Invoke(this)", 3).AppendLine(";")
                .AppendLineWithIndent("}", startIndent);
        }
        void GetDeleteCommands(StringBuilder sb)
        {
            int startIndent = 2;
            sb.AppendWithIndent("public string GetDeleteCommand(Func<", startIndent).Append(tableName).AppendLine(",string> whereClauseMethod)")
                .AppendLineWithIndent("{", startIndent)
                .AppendWithIndent("return $@\"delete from ", 3).Append(this.tableName).AppendLine(" ")
                .AppendWithIndent("{whereClauseMethod.Invoke(this)}", 4).AppendLine(";")
                .AppendLineWithIndent("\";", 4)
                .AppendLineWithIndent("}", startIndent);
        }
        class TupleDef
        {
            public List<string> types = new List<string>();
            public List<string> values = new List<string>();
        }

        public StringBuilder GetClassGenerationString()
        {
            StringBuilder sb = new StringBuilder();
            int startIndent = 1;
            sb.AppendWithIndent("public ", startIndent).Append(this.partial == true ? "partial " : "").Append("class ").Append(this.tableName).AppendLine($@":ICacheble<{this.tableName}>")
                .AppendLineWithIndent("{", startIndent);
            GetExtInsertValues(sb);
            GetExtInsertCustom(sb);
            GetUpdateCommands(sb);
            GetUpdateCommandsCustom(sb);
            GetDeleteCommands(sb);
            sb.AppendLineWithIndent("}", startIndent);
            return sb;
        }
    }
    class ischema_columns
    {
        public string table_name { get; set; }
        public string column_name { get; set; }
        public decimal ORDINAL_POSITION { get; set; }
    }
    public class CrudGenerator
    {
        private string nameSpaceName { get; }
        private List<ClassDeclaration> classes { get; }
        private List<string> namespacesToInclude { get; set; }
        private bool partial { get; set; }
        public CrudGenerator(string nameSpacename, bool partial, List<string> tables, List<string> namespacesToInclude,
            PartnerEntities context)
        {
            this.nameSpaceName = nameSpacename;
            this.namespacesToInclude = namespacesToInclude;
            this.partial = partial;
            classes = new List<ClassDeclaration>();
            List<ischema_columns> lstCols = new List<ischema_columns>();
            Dictionary<string, List<ischema_columns>> attributes = new Dictionary<string, List<ischema_columns>>();
            string sql = $@"SELECT TABLE_NAME,COLUMN_NAME,ORDINAL_POSITION
                            FROM information_schema.columns
                            WHERE TABLE_SCHEMA = 'platinum'; ";
            if (tables == null || tables.Count == 0)
            {
                lstCols = context.Database.SqlQuery<ischema_columns>(sql).ToList();
            }
            else
            {
                lstCols = context.Database.SqlQuery<ischema_columns>(sql +
                                                                    $@" and table_name in({string.Join(",", tables)})"
                                                                    ).ToList();
            }
            lstCols.ForEach(e =>
            {//prepare dictionary
                List<ischema_columns> attrListByTable = null;
                attributes.TryGetValue(e.table_name, out attrListByTable);
                if (attrListByTable == null)
                {
                    attrListByTable = new List<ischema_columns>();
                    attributes.Add(e.table_name, attrListByTable);
                }
                attrListByTable.Add(e);
            });
            foreach (KeyValuePair<string, List<ischema_columns>> kv in attributes)
            {
                classes.Add(new ClassDeclaration(kv.Key, kv.Value.Select(c => c.column_name).ToList(), this.partial));
            }
        }
        private void GenerateAllExceptExtInsertEnums(string targetDir)
        {
            foreach (ClassDeclaration c in this.classes)
            {
                StringBuilder sbThisClass = c.GetClassGenerationString();
                using (StreamWriter sw = new StreamWriter(targetDir + Path.DirectorySeparatorChar + c.tableName + ".cs"))
                {
                    sw.WriteLine(string.Join(Environment.NewLine, namespacesToInclude.Select(n => "using " + n + ";")));
                    sw.Write("namespace ");
                    sw.WriteLine(this.nameSpaceName);
                    sw.WriteLine("{");
                    sw.Write(sbThisClass.ToString());
                    sw.WriteLine("}");
                }
            }
        }
        private void GenerateExtInsertColEnums(string targetDir)
        {
            using (StreamWriter sw = new StreamWriter(targetDir + Path.DirectorySeparatorChar + "_EnumExtInsertCols.cs"))
            {
                sw.WriteLine(string.Join(Environment.NewLine, namespacesToInclude.Select(n => "using " + n + ";")));
                sw.Write("namespace ");
                sw.WriteLine(this.nameSpaceName);
                sw.WriteLine("{");
                sw.WriteLine("\tpublic static class StaticExtInsertCols");
                sw.WriteLine("\t{");
                List<string> enumLines = classes.Select(c => c.GetExtInsertColumns()).ToList();     
                string sep = Environment.NewLine;
                sw.WriteLine(string.Join(sep, enumLines));
                sw.WriteLine("\t}");
                sw.WriteLine("}");
            }
        }
        public void GenerateCrudForAllClasses(string targetDir)
        {
            GenerateAllExceptExtInsertEnums(targetDir);
            GenerateExtInsertColEnums(targetDir);
        }

    }
}

