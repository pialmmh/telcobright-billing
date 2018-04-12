using LibraryExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibraryExtensions.EntityHelper;
namespace TelcobrightTester
{
    class ClassDeclaration
    {
        public string tableNameInDb { get;}
        public string tableNameInCode { get; set; }
        private List<string> columns { get; }
        public bool partial { get; }
        private Func<int, string> GetLineSeparator { get; }
        private string strOverrideForMethods { get; }
        private string strOverrideForProperties { get; }
        public ClassDeclaration(string tableName, List<string> columns, bool partial, bool generatePropertiesAsOverride,
            bool generateMethodsAsOverride)
        {
            this.tableNameInDb = tableName;
            this.columns = columns;
            this.partial = partial;
            this.strOverrideForProperties = generatePropertiesAsOverride == true ? " override " : "";
            this.strOverrideForMethods = generateMethodsAsOverride == true ? " override " : "";
            GetLineSeparator = cIndent => Environment.NewLine + StringBuilderExtensions.GettabStr(cIndent);
        }
        public string GetExtInsertColumns()
        {
            //put everything in one line
            var propertyGet = new StringBuilder().Append($@" insert into ").Append(this.tableNameInDb).Append("(")
                .Append(string.Join(",", columns)).Append(") values ").ToString();
            return new StringBuilder("\t\tpublic static string ")
                .Append(this.tableNameInDb).Append(" { get { return ").Append("\"")
                .Append(propertyGet)
                .Append("\"")
                .Append(";} }").ToString();
        }
        void GetExtInsertValues(StringBuilder sb)
        {
            IEnumerable<string> valuesToMySqlFields = columns.Select(c => new StringBuilder("{").Append(c).Append(".ToMySqlField()}").ToString());
            int startIndent = 2;
            sb.AppendLineWithIndent("public " + strOverrideForMethods + "string GetExtInsertValues()", startIndent)
                .AppendLineWithIndent("{", startIndent)
                .AppendLineWithIndent("return $@\"(", 3)
                .AppendLineWithIndent(string.Join("," + GetLineSeparator(4), valuesToMySqlFields), 4)
                .AppendLineWithIndent(")\";", 4)
                .AppendLineWithIndent("}", startIndent);
        }
        void GetExtInsertCustom(StringBuilder sb)
        {
            int startIndent = 2;
            sb.AppendWithIndent("public " + strOverrideForMethods + " string GetExtInsertCustom(Func<", startIndent).Append(tableNameInCode).Append(",string> externalInsertMethod").AppendLine(")")
                .AppendLineWithIndent("{", startIndent)
                .AppendLineWithIndent("return externalInsertMethod.Invoke(this);", 3)
                .AppendLineWithIndent("}", startIndent);
        }
        void GetUpdateCommands(StringBuilder sb)
        {
            IEnumerable<string> updateLines = columns.Select(c => (new StringBuilder(c).Append("=").Append("{" + c + ".ToMySqlField()+\" \"" + "}"))
                                                                  .ToString());
            int startIndent = 2;
            sb.AppendWithIndent("public " + strOverrideForMethods + " string GetUpdateCommand(Func<", startIndent).Append(tableNameInCode).AppendLine(",string> whereClauseMethod)")
                .AppendLineWithIndent("{", startIndent)
                .AppendWithIndent("return $@\"update ", 3).Append(this.tableNameInDb).AppendLine(" set ")
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
            sb.AppendWithIndent("public " + strOverrideForMethods + " string GetUpdateCommandCustom(Func<", startIndent).Append(tableNameInCode).AppendLine(",string> updateCommandMethodCustom)")
            .AppendLineWithIndent("{", startIndent)
                .AppendWithIndent("return updateCommandMethodCustom.Invoke(this)", 3).AppendLine(";")
                .AppendLineWithIndent("}", startIndent);
        }
        void GetDeleteCommands(StringBuilder sb)
        {
            int startIndent = 2;
            sb.AppendWithIndent("public " + strOverrideForMethods + " string GetDeleteCommand(Func<", startIndent).Append(tableNameInCode).AppendLine(",string> whereClauseMethod)")
                .AppendLineWithIndent("{", startIndent)
                .AppendWithIndent("return $@\"delete from ", 3).Append(this.tableNameInDb).AppendLine(" ")
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
            sb.AppendWithIndent("public ", startIndent).Append(this.partial == true ? "partial " : "").Append("class ").Append(this.tableNameInCode).AppendLine($@":ICacheble<{this.tableNameInCode}>")
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
}
