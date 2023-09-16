using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
namespace TelcobrightInfra
{
    public class SqlAggregator
    {
        private string NonUnionSql { get; set; }
        private string BaseSql { get; set; }
        private string HeaderSql { get; set; }
        private string TrailerSql { get; set; }
        private char SpaceSubstitutionForColNames { get; set; } = '~';
        private string baseSqlStartsWith;
        private string baseSqlEndsWith;
        private string groupByExpression;
        public List<string> TableNames { get; set; }
        public List<DbColumn> headerColumns = new List<DbColumn>();
        public Dictionary<string, DbColumn> baseColumns = new Dictionary<string, DbColumn>();
        private string newLine = "\r\n";

        public SqlAggregator(string nonUnionSql, IEnumerable<string> tableNames,
            string _baseSqlStartsWith, string _baseSqlEndsWith)
        {
            if (nonUnionSql.Contains("<basetable>") == false)
                throw new Exception("Non union sql must contain <basetable>");
            if (nonUnionSql.Contains(this.SpaceSubstitutionForColNames) == true)
                throw new Exception("Non union sql cannot contain " + this.SpaceSubstitutionForColNames);
            this.baseSqlStartsWith = _baseSqlStartsWith;
            this.baseSqlEndsWith = _baseSqlEndsWith;
            this.NonUnionSql = replaceCharWithinSingleQuotes(nonUnionSql, ' ', this.SpaceSubstitutionForColNames);
            this.NonUnionSql = makeKeyWordsLowerCase(this.NonUnionSql);//make all select lower case, dont' change case of col names
            this.BaseSql = BaseSql;
            this.TableNames = tableNames.ToList();

            List<string> sqlParts = this.NonUnionSql.Split(new string[] { "from", "FROM", "From" }, StringSplitOptions.None).ToList();
            string columnsLineInHeader = ReplaceFirstOccurance(sqlParts[0].TrimStart('('), "select", "");
            this.headerColumns = getColumnsFromSelectLine(columnsLineInHeader);//header columns

            string sqlAfterHeader = sqlParts[1].ToLower() + " from " + sqlParts[2].ToLower();
            string[] tempStr = sqlAfterHeader.Split(new string[] { "left join" }, StringSplitOptions.None);
            string startIdentifierRemoved = tempStr[0].Substring(tempStr[0].IndexOf(this.baseSqlStartsWith) + 1);

            string[] baseSqlAndGroupByBuilder = startIdentifierRemoved.Substring(0, startIdentifierRemoved.LastIndexOf(this.baseSqlEndsWith))
                .Split(new[] { "group by" }, StringSplitOptions.None);
            this.BaseSql = baseSqlAndGroupByBuilder[0];//baseSql
            this.groupByExpression = string.Join(" ", baseSqlAndGroupByBuilder.Skip(1));
            this.BaseSql = this.BaseSql + " group by " + this.groupByExpression;
            this.TrailerSql = " left join " + string.Join(" left join ", tempStr.Skip(1));//trailerSql

            sqlParts = this.BaseSql.Split(new string[] { "from", "FROM", "From" }, StringSplitOptions.None).ToList();
            string columnsLineInBaseSql = ReplaceFirstOccurance(sqlParts[0].TrimStart('('), "select", "")
                .Split(new string[] { "group by" }, StringSplitOptions.None)[0];
            this.baseColumns = getColumnsFromSelectLine(columnsLineInBaseSql).ToDictionary(c => c.ColName.ToLower());//base columns: tolower() is necessary because the first column line may conatain 'TotalCalls', whereas base sql has totalcalls

            //identify summable columns in header column and set flag
            this.headerColumns.ForEach(hCol =>
            {
                DbColumn matchedBaseCol = null;
                if (this.baseColumns.TryGetValue(hCol.ColExpression.ToLower(), out matchedBaseCol))//base colnames.tolower() is used as the baseColumns key
                {
                    hCol.IsSummable = matchedBaseCol.IsSummable;
                }
            });
        }

        private string makeKeyWordsLowerCase(string str)
        {
            str = Regex.Replace(str, "SELECT", "select", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, "GROUP BY", "group by", RegexOptions.IgnoreCase);
            return str;
        }

        public string ReplaceFirstOccurance(string str, string search, string replace)
        {
            int pos = str.IndexOf(search);
            if (pos < 0)
            {
                return str;
            }
            return str.Substring(0, pos) + replace + str.Substring(pos + search.Length);
        }

        private string replaceCharWithinSingleQuotes(string sql, char charToReplace, char replaceWith)
        {
            var tempArr = sql.ToCharArray();
            bool insideColName = false;
            for (var index = 0; index < tempArr.Length; index++)
            {
                var c = tempArr[index];
                if (c == '\'')
                {
                    insideColName = !insideColName;
                    continue;
                }

                if (insideColName)
                {
                    if (c == charToReplace)
                    {
                        tempArr[index] = replaceWith;
                    }
                }
            }
            return new string(tempArr);
        }
        private static string replaceCharWithinFristBrace(string sql, int startFrom, char charToReplace, char replaceWith)
        {
            var tempArr = sql.ToCharArray();
            bool insideColName = false;
            int braceCount = 0;
            for (var index = startFrom; index < tempArr.Length; index++)
            {
                var c = tempArr[index];
                if (c == '(' || c == ')')
                {
                  
                    if (c == '(')
                    {
                        braceCount++;
                        insideColName = true;
                    }
                    else
                    {
                        braceCount--;
                    }
                    if (braceCount == 0)
                    {
                        insideColName = false;
                        break;
                    }
                }         
                else
                {
                    if (insideColName)
                    {
                        if (c == charToReplace)
                        {
                            tempArr[index] = replaceWith;
                        }
                    }
                    continue;
                }
                
            }
            return new string(tempArr);
        }


        private StringBuilder wrapUnionedSqlWithGroupBy(string unionedSql)
        {
            StringBuilder colsInBaseSqlWrapper =
                new StringBuilder("(")
                    .Append($"select " + string.Join(",", this.baseColumns.Values.Select(c => c.ToBaseSqlWrapperString()))).Append(newLine)
                    .Append($" FROM").Append(newLine)
                    .Append(unionedSql).Append(newLine)
                    .Append(" group by ").Append(this.groupByExpression).Append(newLine)
                    .Append(this.baseSqlEndsWith);
            return colsInBaseSqlWrapper;
        }

        public string getFinalSql()
        {
            string unionedSql = new StringBuilder("(")
                .Append(string.Join($"{newLine}UNION ALL{newLine}",
                    this.TableNames.Select(t => replaceTableNameIcxName(t))))
                .Append(") as unioned").ToString();
            StringBuilder unionSqlWrapped = wrapUnionedSqlWithGroupBy(unionedSql);

            StringBuilder headerSql =
                new StringBuilder($"select " + string.Join(",", this.headerColumns.Select(c => c.ToHeaderString()))).Append(newLine)
                    .Append($" FROM").Append(newLine);

            string finalSql = headerSql.Append(newLine)
                .Append(unionSqlWrapped).Append(newLine)
                .Append(this.TrailerSql).ToString();
            finalSql = this.replaceCharWithinSingleQuotes(finalSql, '~', ' ');
            return finalSql;
        }

        private string replaceTableNameIcxName(string t)
        {
            return this.BaseSql.Replace("<basetable>",t).Replace("_icxname_", t.Split('.')[0].Split('_')[0]);
          
        }

        private static List<DbColumn> getColumnsFromSelectLine(string selectLine)
        {
            //var columns = new List<DbColumn>();
            if (selectLine.Contains("concat(") == true)
            {
                int startIndex = selectLine.IndexOf(@"concat(");
                selectLine = replaceCharWithinFristBrace(selectLine, startIndex, ',', '~');
            }

            if (selectLine.Contains("date_format(") == true)
            {
                int startIndex = selectLine.IndexOf(@"date_format(");
                selectLine = replaceCharWithinFristBrace(selectLine, startIndex, ',', '~');
            }
            var columns = selectLine.Split(',')
                .Select(colNameWithExp =>
                {
                    string[] colAsArr = colNameWithExp.Split(null)
                        .Select(c => c.Trim())
                        .Where(item => string.IsNullOrWhiteSpace(item) == false &&
                                       string.IsNullOrEmpty(item) == false)
                        .Where(item => item.ToLower() != "as").ToArray();
                    //remove 'as' from colExpression, added later in toString or toHeaderString()
                    var colExpression = colAsArr.Length > 1 ? string.Join(" ", colAsArr.Take(colAsArr.Length - 1)) : colAsArr.Last();
                    var Dbcolumn = new DbColumn
                    {
                        ColName = colAsArr.Last(),
                        ColExpression = colExpression,
                        IsSummable = colExpression.Contains("sum(")
                    };
                    return Dbcolumn;
                }).ToList();
            var dups = columns.GroupBy(c => c.ColName).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            return columns;
        }
    }
}
