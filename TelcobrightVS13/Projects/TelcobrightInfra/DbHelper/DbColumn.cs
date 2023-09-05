using System;
using System.Text;

namespace TelcobrightInfra
{
    public class DbColumn
    {
        public string ColName { get; set; }
        public string ColExpression { get; set; }
        public bool IsSummable { get; set; }

        public string HeaderAlias { get; set; }
        //public string ToHeaderString()
        //{
        //    Func<string> getColIfNameEqualsExp = () => this.ColExpression == this.ColName
        //         ? this.ColName
        //         : new StringBuilder(this.ColExpression).Append(" as ").Append(this.ColName).ToString();

        //    return this.IsSummable == true
        //        ? new StringBuilder("sum(").Append(this.ColExpression).Append(") as ").Append(this.ColName).ToString()
        //        : getColIfNameEqualsExp();
        //}

        public string ToHeaderString()
        {
            return this.ColName == this.ColExpression
                ? this.ColName
                : new StringBuilder(this.ColExpression).Append(" as ").Append(this.ColName).ToString();
        }

        public string ToBaseSqlWrapperString()
        {
            Func<string> getColIfNameEqualsExp = () => this.ColExpression == this.ColName
                ? this.ColName
                : new StringBuilder(this.ColExpression).Append(" as ").Append(this.ColName).ToString();

            return this.IsSummable == true
                ? new StringBuilder("sum(").Append(this.ColName).Append(") as ").Append(this.ColName).ToString()
                : this.ColName;
        }

        public override string ToString()
        {
            return this.ColName;
        }
    }
}