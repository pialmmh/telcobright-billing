using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
namespace Utils.Testers.Employees
{
    public class FullTextSearchTester
    {
        private List<string> _textsToSearch = new List<string>() { "ant", "ous", "kus", "law" };
        private employeesEntities _context = new employeesEntities();
        private int _repeatCount = 500;
        public void Test()
        {
            List<string> searchStrs = GetSearchPatternBySubString();
            
            var start = DateTime.Now;
            int matchedCount = 0;
            for (int i = 0; i < _repeatCount; i++)
            {
                string query = GetQueryWithoutFullText(searchStrs, i);
                if (this._context.Database.Connection.State != ConnectionState.Open)
                {
                    this._context.Database.Connection.Open();
                }
                DbCommand cmd = this._context.Database.Connection.CreateCommand();
                cmd.CommandText = query;
                DbDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    matchedCount += Convert.ToInt32(reader[0].ToString());
                }
                reader.Close();
                Console.WriteLine("Execution count= "+i.ToString());
            }
            var end = DateTime.Now;
            Console.WriteLine("Time Elapsed="+ (end-start).TotalSeconds + " Seconds");
        }

        private static string GetQueryWithoutFullText(List<string> searchStrs, int i)
        {
            return $@"select count(*) from employees 
                            where first_name like '%{searchStrs[i]}%' 
                            or last_name like '%{searchStrs[i]}%'";
        }

        private List<string> GetSearchPatternBySubString()
        {
            List<string> searchPatterns = this._context.employees.Select(e => e.first_name + " " + e.last_name).ToList()
                .Select(name => name.Substring(3, 4).Trim()).Take(this._repeatCount).ToList();


            return searchPatterns;
        }

    }
}
