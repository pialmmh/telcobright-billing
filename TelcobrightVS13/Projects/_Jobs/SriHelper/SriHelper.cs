using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using MediationModel;
using LibraryExtensions.ConfigHelper;
using TelcobrightMediation;
using System.Linq;
using TelcobrightMediation.Cdr;
using LibraryExtensions;
using System.Text;

namespace Jobs
{
    class SriHelper
    {
        //private PartnerEntities Context { get; }
        public TelcobrightConfig Tbc { get; }
        public DatabaseSetting DatabaseSetting { get; }
        private readonly string connectionString;
        private MySqlConnection Conn { get; set; }
        private string PartialTableName = "zz_zz_partialevent_sri";
        private string SriTableName = "sri";
        private DateTime StartTime = new DateTime();
        private DateTime EndTime = new DateTime();
        private List<DateTime> daysList = new List<DateTime>();
        public List<string[]> TxtCdrRows { get; set; } = new List<string[]>();
        public List<string[]> NewRowsCouldNotBeAggregated { get; set; } = new List<string[]>();
        public List<string[]> AggregatedSriRows { get; set; } = new List<string[]>();
        //public Dictionary<DateTime, List<string[]>> DayWiseTxtCdrRows { get; set; } = new Dictionary<DateTime, List<string[]>>();
        public List<string[]> OldRowsToBeDiscardedAfterAggregation { get; } = new List<string[]>();

        public SriHelper(TelcobrightConfig tbc, NewCdrPreProcessor preProcessor, List<string[]> txtRows)
        {
            this.DatabaseSetting = tbc.DatabaseSetting;
            this.Tbc = tbc;
            this.connectionString = $"Server={this.DatabaseSetting.ServerName};Database=smshub;User ID={this.DatabaseSetting.WriteUserNameForApplication};Password={this.DatabaseSetting.WritePasswordForApplication};SslMode=none;";
            this.TxtCdrRows = txtRows.Where(txt => txt[Sn.SmsType] == "1" || txt[Sn.SmsType] == "2").ToList();
            if (TxtCdrRows.Count == 0) return;
            this.StartTime = TxtCdrRows
                .Min(cdr => DateTime.ParseExact(cdr[Fn.StartTime], "yyyy-MM-dd HH:mm:ss", null));
            this.EndTime = TxtCdrRows
                .Max(cdr => DateTime.ParseExact(cdr[Fn.StartTime], "yyyy-MM-dd HH:mm:ss", null));
            CreateTable();
        }

        public void InitializeDbConnection(MySqlConnection conn)
        {
            this.Conn = conn;
        }

        public void CreateTable()
        {
            string tableName = this.PartialTableName;
            string query = $@"CREATE TABLE IF NOT EXISTS {tableName} (
                            IdCall  bigint(20) NOT NULL,
                            FileName  varchar(70) COLLATE utf8mb4_bin NOT NULL,
                            TerminatingCalledNumber varchar(20) COLLATE utf8mb4_bin DEFAULT NULL,
                            EndTime  datetime DEFAULT NULL,
                            ChargingStatus  tinyint(1) DEFAULT NULL,
                            StartTime  datetime DEFAULT NULL,
                            PartialFlag  tinyint(1) DEFAULT NULL,
                            AdditionalSystemCodes  varchar(30) COLLATE utf8mb4_bin DEFAULT NULL,
                            Codec varchar(10) COLLATE utf8mb4_bin DEFAULT NULL,
                            RedirectingNumber  varchar(20) COLLATE utf8mb4_bin DEFAULT NULL,
                            Duration3  tinyint(1) unsigned DEFAULT NULL,
                            UniqueBillId  varchar(30) COLLATE utf8mb4_bin DEFAULT NULL,
                            E1Id int(11) DEFAULT NULL,
                            PRIMARY KEY (IdCall),
                            KEY  ind_Unique_Bill  ( UniqueBillId ),
                            KEY ind_Start_Time (StartTime)
                        ) ENGINE=InnoDB
                        DATA DIRECTORY = 'C:/mysql/data'";

            ExecuteNonQuery(query);

            // Console.WriteLine("Table 'PartialEvent' created (if not exists).");
        }

        public void InsertFailedRows()
        {
            int batchSize = this.Tbc.CdrSetting.SegmentSizeForDbWrite;
            if (this.TxtCdrRows.Count == 0) return;

            string tableName = this.PartialTableName;

            int totalRows = this.NewRowsCouldNotBeAggregated.Count;
            int insertCount = 0;
            var rows = this.NewRowsCouldNotBeAggregated;

            for (int i = 0; i < totalRows; i += batchSize)
            {
                var batch = rows.Skip(i).Take(batchSize).ToList();  // Get a segment of the rows
                var stringBuilders = batch.Select(c => GetFailedExtInsertValues(c)).ToList();
                string InsertExpression = $"insert into {tableName}(IdCall,FileName,TerminatingCalledNumber,EndTime,ChargingStatus,StartTime,PartialFlag,AdditionalSystemCodes,Codec,RedirectingNumber,Duration3,UniqueBillId, E1Id) values";

                string sql = InsertExpression + string.Join(",", stringBuilders);
                sql = sql.EndsWith(";") ? sql : new StringBuilder(sql).Append(";").ToString();

                // Now use the ExecuteQuery method to execute the SQL query
                insertCount += ExecuteQuery(sql);
            }

            // Ensure the number of inserted records matches the expected count
            if (insertCount != totalRows)
            {
                throw new Exception("Inserted number of SRIs do not match raw count in collection result.");
            }

            // Console.WriteLine("Rows inserted successfully in batches.");
        }

        public void InsertSuccessfulRows()
        {
            int batchSize = this.Tbc.CdrSetting.SegmentSizeForDbWrite;
            if (this.TxtCdrRows.Count == 0) return;

            string tableName = this.SriTableName;

            int totalRows = this.AggregatedSriRows.Count;
            int insertCount = 0;
            var rows = this.AggregatedSriRows;

            for (int i = 0; i < totalRows; i += batchSize)
            {
                var batch = rows.Skip(i).Take(batchSize).ToList();  // Get a segment of the rows
                var stringBuilders = batch.Select(c => GetSuccessfulExtInsertValues(c)).ToList();
                string InsertExpression = $"INSERT INTO {tableName} (TerminatingCalledNumber, StartTime, RedirectingNumber) VALUES";

                string sql = InsertExpression + string.Join(",", stringBuilders);
                sql = sql.EndsWith(";") ? sql : new StringBuilder(sql).Append(";").ToString();

                // Now use the ExecuteQuery method to execute the SQL query
                insertCount += ExecuteQuery(sql);
            }

            // Ensure the number of inserted records matches the expected count
            if (insertCount != totalRows)
            {
                throw new Exception("Inserted number of SRIs do not match raw count in collection result.");
            }

            // Console.WriteLine("Rows inserted successfully in batches.");
        }

        public List<string[]> FetchFailedRows()
        {
            List<string[]> rows = new List<string[]>();

            // Return an empty list if there are no rows to fetch
            if (this.TxtCdrRows.Count == 0) return rows;

            string tableName = this.PartialTableName;
            DateTime adjustedStartTime = this.StartTime.AddMinutes(-1);
            DateTime adjustedEndTime = this.EndTime.AddMinutes(1);

            // Fetch all rows at once, ordered by IdCall for consistency
            string query = $@"
        SELECT * FROM {tableName}
        WHERE StartTime >= '{adjustedStartTime:yyyy-MM-dd HH:mm:ss}'
        AND StartTime <= '{adjustedEndTime:yyyy-MM-dd HH:mm:ss}';";
        //ORDER BY IdCall;";  // Ordering ensures stable fetching

            // Execute the query and fetch results using ExecuteReader
            rows = ExecuteReader(query);

            return rows;
        }

        public void DeleteFailedRows()
        {
            var idCallsToDelete = this.OldRowsToBeDiscardedAfterAggregation
                                .Select(s => long.Parse(s[Sn.IdCall]))  // Assuming you're extracting IdCall from the rows
                                .ToList();
            if (idCallsToDelete.Count == 0) return;
            string tableName = this.PartialTableName;
            int delCount = 0;
            int batchSize = this.Tbc.CdrSetting.SegmentSizeForDbWrite;

            for (int i = 0; i < idCallsToDelete.Count; i += batchSize)
            {
                var batch = idCallsToDelete.Skip(i).Take(batchSize).ToList();
                string idCallList = string.Join(",", batch);
                string deleteQuery = $@"
                                    DELETE FROM {tableName} 
                                    WHERE IdCall IN ({idCallList})";

                delCount += ExecuteQuery(deleteQuery);
            }

            // Ensure the number of deleted records matches the expected count
            if (delCount != idCallsToDelete.Count)
            {
                throw new Exception("Deleted number of SRIs do not match raw count in collection result.");
            }

            DeleteOldFailedRows();
        }

        public void DeleteOldFailedRows()
        {
            var deleteBeforeTime = this.StartTime.AddMinutes(-5);
            string tableName = this.PartialTableName;

            string deleteQuery = $@"
                DELETE FROM {tableName} 
                WHERE StartTime < '{deleteBeforeTime:yyyy-MM-dd HH:mm:ss}'";

            ExecuteQuery(deleteQuery);
        }

        private int ExecuteQuery(string query, params MySqlParameter[] parameters)
        {
            using (MySqlCommand cmd = new MySqlCommand(query, this.Conn))  // Remove the transaction reference
            {
                if (parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);  // Add parameters to the command
                return cmd.ExecuteNonQuery();  // Execute the query without a transaction context
            }
        }

        private void ExecuteNonQuery(string query, params MySqlParameter[] parameters)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    if (parameters.Length > 0)
                        cmd.Parameters.AddRange(parameters);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private List<string[]> ExecuteReader(string query)
        {
            List<string[]> rows = new List<string[]>();

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string[] row = new string[150];  // Adjust the size as needed
                        row[Fn.IdCall] = reader[0].ToString();
                        row[Fn.Filename] = reader[1].ToString();
                        row[Fn.TerminatingCalledNumber] = reader[2].ToString();
                        row[Fn.Endtime] = reader[3].ToString();
                        row[Fn.ChargingStatus] = reader[4].ToString();
                        row[Fn.StartTime] = reader[5].ToString();
                        row[Fn.Partialflag] = reader[6].ToString();
                        row[Fn.AdditionalSystemCodes] = reader[7].ToString();
                        row[Fn.Codec] = reader[8].ToString();
                        row[Fn.Redirectingnumber] = reader[9].ToString();
                        row[Fn.Duration3] = reader[10].ToString();
                        row[Fn.UniqueBillId] = reader[11].ToString();
                        row[Fn.E1Id] = reader[12].ToString();
                        rows.Add(row);
                    }
                }
            }

            return rows;
        }

        public StringBuilder GetFailedExtInsertValues(string[] t)
        {
            return new StringBuilder("(")
                .Append(t[Fn.IdCall].ToMySqlField()).Append(",")
                .Append(t[Fn.Filename].ToMySqlField()).Append(",")
                .Append(t[Fn.TerminatingCalledNumber].ToMySqlField()).Append(",")
                .Append(t[Fn.StartTime].ToMySqlField()).Append(",") // assigning endtime as startime
                .Append(t[Fn.ChargingStatus].ToMySqlField()).Append(",")
                .Append(t[Fn.StartTime].ToMySqlField()).Append(",")
                .Append(t[Fn.Partialflag].ToMySqlField()).Append(",")
                .Append(t[Fn.AdditionalSystemCodes].ToMySqlField()).Append(",")
                .Append(t[Fn.Codec].ToMySqlField()).Append(",")
                .Append(t[Fn.Redirectingnumber].ToMySqlField()).Append(",")
                .Append(t[Fn.Duration3].ToMySqlField()).Append(",")
                .Append(t[Fn.UniqueBillId].ToMySqlField()).Append(",")
                .Append(t[Fn.E1Id].ToMySqlField()).Append(")");

        }

        public StringBuilder GetSuccessfulExtInsertValues(string[] t)
        {
            return new StringBuilder("(")
                .Append(t[Fn.TerminatingCalledNumber].ToMySqlField()).Append(",")
                .Append(t[Fn.StartTime].ToMySqlField()).Append(",")
                .Append(t[Fn.Redirectingnumber].ToMySqlField()).Append(")");
        }
    }
}
