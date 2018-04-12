using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Spring.Objects.Factory.Config;
using LibraryExtensions;

namespace Utils
{
    class CsvImporter
    {
        private string path = "C:\\Dropbox\\partial testing csvs with fake cdr\\mockcdrs";
        private string databaseName = "mockcdr";

        private MySqlConnection con = new MySqlConnection(ConfigurationManager.ConnectionStrings["partner"]
            .ConnectionString.Replace("platinum", "mockcdr"));
        int ColCountInRawCdr = 170;
        public CsvImporter()
        {
            con.Open();
        }

        public void Import()
        {
            var dir=new DirectoryInfo(path);
            List<string> singleInserts = new List<string>();
            foreach (FileInfo file in dir.GetFiles())
            {
                List<string[]> lines = GetTxtTableByCsvParsing(file.FullName, ';');
                string sql = "insert into dialogiccdr values ";
                foreach (string[] line in lines)
                {
                    singleInserts.Add(lineToExtInsert(line,file.Name));
                }
                sql += string.Join("," + Environment.NewLine, singleInserts);
                using (MySqlCommand cmd = new MySqlCommand(sql, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        string lineToExtInsert(string[] line,string fileName)
        {
            return "(" + string.Join(",", line.Select(c => c.EncloseWith("'")))+"," +fileName.EncloseWith("'")+ ")";
        }

        List<string[]> GetTxtTableByCsvParsing(string fileName,char delimeter)
        {
            TextReader textReader = new StreamReader(fileName);
            List<string[]> lines= new List<string[]>();
            string line;
            while ((line=textReader.ReadLine())!=null)
            {
                lines.Add(line.Split(';'));
            }
            for (int i = 0; i < lines.Count; i++)
            {
                int colCount = lines[i].GetLength(0);
                if (colCount < this.ColCountInRawCdr)
                {
                    int NoOfColsToPad = this.ColCountInRawCdr - colCount;
                    string[] resizedArr = null;
                    Array.Resize(ref resizedArr,colCount+NoOfColsToPad);
                    lines[i].CopyTo(resizedArr,0);
                    lines[i] = resizedArr;
                }
            }
            return lines;
        }
        public void createTable(string tableName)
        {
            string sql=new StringBuilder("create table ").Append(tableName).Append("(").ToString();
            List<string> colDefs=new List<string>();
            for (int i = 0; i <= (ColCountInRawCdr); i++)//filename=171st column
            {
                colDefs.Add(" column"+(i+1)+" varchar(100) null ");    
            }
            colDefs.Add(" id int primary key auto_increment ");
            MySqlCommand cmd=new MySqlCommand("",con);
            cmd.CommandText = sql + string.Join("," + Environment.NewLine, colDefs)+")";
            cmd.ExecuteNonQuery();
        }
    }
}
