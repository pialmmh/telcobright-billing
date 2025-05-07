using LibraryExtensions;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelcobrightMediation;
using TelcobrightMediation.Config;

namespace MatchMtSri
{
    class ImsiListener
    {
        private readonly string connectionStringSri;
        private readonly string connectionStringMt;
        private DateTime StartTime = new DateTime();
        private DateTime NewStartTime = new DateTime();
        static readonly string topshelfDir = new UpwordPathFinder<DirectoryInfo>("WS_Topshelf_Quartz").FindAndGetFullPath();
        private readonly TelcobrightConfig tbc;


        public ImsiListener()
        {
            string configFilePath = Path.Combine(topshelfDir, "deployedInstances");
            string configFileName = Directory.GetFiles(configFilePath, "*.conf", SearchOption.AllDirectories).First();
            this.tbc = ConfigFactory.GetConfigFromFile(configFileName);
            this.connectionStringSri = $"Server={this.tbc.DatabaseSetting.ServerName};Database={this.tbc.DatabaseSetting.DatabaseName};User ID={this.tbc.DatabaseSetting.WriteUserNameForApplication};Password={this.tbc.DatabaseSetting.WritePasswordForApplication};SslMode=none;";
            this.connectionStringMt = $"Server=172.20.23.105;Database=smshub;User ID={this.tbc.DatabaseSetting.WriteUserNameForApplication};Password={this.tbc.DatabaseSetting.WritePasswordForApplication};SslMode=none;";
        }

        public void SetStartTime(DateTime startTime)
        {
            this.StartTime = startTime;
        }

        public List<string[]> FetchCdrRows()
        {
            List<string[]> rows = new List<string[]>();
            string tableName = "cdr";
            DateTime startTime = this.StartTime;
            DateTime endTime = this.StartTime.AddSeconds(600);
            this.NewStartTime = endTime.AddSeconds(1);

            // Embedded datetime values in SQL
            string query = $@"
        SELECT IdCall, RedirectingNumber, TerminatingCallingNumber, StartTime
        FROM {tableName}
        WHERE StartTime >= '{startTime.ToString("yyyy-MM-dd HH:mm:ss")}'
          AND StartTime <= '{endTime.ToString("yyyy-MM-dd HH:mm:ss")}'
          AND RedirectingNumber IS NOT NULL
        ORDER BY IdCall ASC;";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(this.connectionStringMt))
                {
                    conn.Open();

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.CommandTimeout = 300;

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    string[] row = new string[150]; // Adjust if needed
                                    row[Fn.IdCall] = reader[0].ToString();
                                    row[Fn.Redirectingnumber] = reader[1].ToString();
                                    row[Fn.TerminatingCallingNumber] = reader[2].ToString();
                                    row[Fn.StartTime] = reader[3].ToString();
                                    rows.Add(row);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while fetching data: {ex.Message}");
            }

            return rows;
        }

        public List<string[]> FetchSriRows(List<string> redirectingNumber)
        {
            List<string[]> rows = new List<string[]>();
            string tableName = "sri";
            DateTime startTime = this.StartTime.AddSeconds(-300);
            DateTime endTime = this.StartTime.AddSeconds(600);

            if (redirectingNumber == null || redirectingNumber.Count == 0)
            {
                Console.WriteLine("No redirecting numbers provided.");
                return rows;
            }

            // Build the IN clause without sanitizing
            string inClause = string.Join(",", redirectingNumber.Select(n => $"'{n}'"));

            string query = $@"
        SELECT * FROM {tableName}
        WHERE StartTime >= '{startTime.ToString("yyyy-MM-dd HH:mm:ss")}'
          AND StartTime <= '{endTime.ToString("yyyy-MM-dd HH:mm:ss")}'
          AND RedirectingNumber IN ({inClause});";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(this.connectionStringSri))
                {
                    conn.Open();

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.CommandTimeout = 300;

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    string[] row = new string[150]; // Adjust size if needed
                                    row[Fn.TerminatingCalledNumber] = reader[0].ToString();
                                    row[Fn.StartTime] = reader[1].ToString();
                                    row[Fn.Redirectingnumber] = reader[2].ToString();
                                    rows.Add(row);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while fetching data: {ex.Message}");
            }

            return rows;
        }

        public void UpdateBPartySyncTime()
        {
            string query = $"UPDATE b_party_sync_time SET startTime = '{this.NewStartTime.ToString("yyyy-MM-dd HH:mm:ss")}'";

            using (MySqlConnection conn = new MySqlConnection(this.connectionStringSri))
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    int affectedRows = cmd.ExecuteNonQuery();

                    if (affectedRows == 1)
                    {
                        Console.WriteLine($"[BPartyListener] Sync time updated to {this.NewStartTime:yyyy-MM-dd HH:mm:ss}.");
                    }
                    else
                    {
                        throw new Exception($"[BPartyListener] Sync time update failed. No rows were affected. Expected 1 row to be updated.");
                    }
                }
            }
        }

        public List<string[]> aggregateMtWithSri(List<string[]> aggSri, List<string[]> aggMt)
        {
            aggSri = consistantRows(aggSri);
            aggMt = consistantRows(aggMt);
            List<DateTime> dates = aggMt.AsParallel()
                                       .Select(r => r[Fn.StartTime].ConvertToDateTimeFromMySqlFormat())
                                       .ToList();

            // Create a dictionary for fast lookup of aggSri by Imsi
            var sriDictionary = aggSri
                .GroupBy(sri => sri[Fn.Redirectingnumber])
                .ToDictionary(g => g.Key, g => g.ToList());

            // List to track items to remove from aggins
            var itemsToRemove = new HashSet<string>();
            var maxDiff = 0;

            // Process each element in aggMt
            foreach (var mt in aggMt)
            {
                var sriMatches = new List<string[]>();
                bool flag = false;
                if (sriDictionary.TryGetValue(mt[Fn.Redirectingnumber], out sriMatches))
                {
                    foreach (var sri in sriMatches)
                    {
                        DateTime mtTime = Convert.ToDateTime(mt[Fn.StartTime]);
                        DateTime sriTime = Convert.ToDateTime(sri[Fn.StartTime]);
                        maxDiff = Math.Max(maxDiff, (int)(mtTime - sriTime).TotalSeconds);
                        if ((mtTime - sriTime).TotalSeconds >= 0 && (mtTime - sriTime).TotalSeconds <= 300)
                        {
                            mt[Fn.TerminatingCalledNumber] = sri[Fn.TerminatingCalledNumber];
                            itemsToRemove.Add(sri[Fn.UniqueBillId]);
                            flag = true;
                            break;
                        }
                    }
                }
                if(!flag)
                {
                    ; //test for missing B Party
                }
            }

            return aggMt;
        }

        public DateTime GetBPartySyncTime()
        {
            string query = "SELECT StartTime FROM b_party_sync_time LIMIT 1";
            using (MySqlConnection conn = new MySqlConnection(this.connectionStringSri))
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetDateTime("StartTime");
                        }
                        else
                        {
                            throw new InvalidOperationException("No sync time found in b_party_sync_time table.");
                        }
                    }
                }
            }
        }

        public void updateTerminatingCalledNumberInCdrTable(List<string[]> aggMt, int batchSize)
        {
            var tableName = "cdr";
            if (aggMt == null || aggMt.Count == 0)
                return;

            using (MySqlConnection conn = new MySqlConnection(this.connectionStringMt))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    // Start manual transaction mode
                    cmd.CommandText = "SET autocommit = 0;";
                    cmd.ExecuteNonQuery();

                    int totalRows = aggMt.Count;
                    int batchCount = (totalRows + batchSize - 1) / batchSize;
                    int totalUpdatedRows = 0;

                    try
                    {
                        for (int batchIndex = 0; batchIndex < batchCount; batchIndex++)
                        {
                            var batchRows = aggMt.Skip(batchIndex * batchSize).Take(batchSize).ToList();
                            var updates = new List<string>();
                            var idCalls = new List<string>();

                            foreach (var row in batchRows)
                            {
                                string idCall = row[Fn.IdCall].Replace("'", "''");
                                string terminatingCalledNumber = row[Fn.TerminatingCalledNumber].Replace("'", "''");

                                updates.Add($"WHEN {idCall} THEN '{terminatingCalledNumber}'");
                                idCalls.Add(idCall);
                            }

                            if (updates.Count > 0)
                            {
                                cmd.CommandText = $@"
                            UPDATE {tableName}
                            SET TerminatingCalledNumber = CASE idCall
                                {string.Join(" ", updates)}
                            END
                            WHERE idCall IN ({string.Join(",", idCalls)});
                        ";

                                cmd.CommandTimeout = 3600;
                                totalUpdatedRows += cmd.ExecuteNonQuery();
                            }
                        }
                        UpdateBPartySyncTime();
                        cmd.CommandText = "COMMIT;";
                        cmd.ExecuteNonQuery();

                        Console.WriteLine($"[BPartyListener] Total CDR rows updated with B Party: {totalUpdatedRows}");
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            cmd.CommandText = "ROLLBACK;";
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception rollbackEx)
                        {
                            Console.WriteLine("Rollback failed: " + rollbackEx.Message);
                        }

                        Console.WriteLine("Update failed: " + ex.Message);
                        throw;
                    }
                }
            }
        }

        private string ConvertToMySqlFormat(string dateStr)
        {
            // Define the input date formats explicitly.
            var formats = new[] { "M/d/yyyy h:mm:ss tt", "yyyy-MM-dd HH:mm:ss" };  // Using "h" for single-digit hours in 12-hour format

            DateTime parsedDate;
            foreach (var format in formats)
            {
                // Try to parse the input date string with the defined formats and invariant culture
                if (DateTime.TryParseExact(dateStr, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                {
                    // Return the date in MySQL compatible format (yyyy-MM-dd HH:mm:ss)
                    return parsedDate.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }

            // If the date could not be parsed, throw an exception
            throw new FormatException($"Invalid date format: {dateStr}");
        }

        public void DeleteOldData()
        {
            // Format the cutoff time
            string cutoffTime = this.StartTime.AddHours(-1).ToString("yyyy-MM-dd HH:mm:ss");

            // Inline SQL with the timestamp embedded directly
            string query = $"DELETE FROM sri WHERE StartTime < '{cutoffTime}'";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(this.connectionStringSri))
                {
                    conn.Open();

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.CommandTimeout = 3600;
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            Console.WriteLine($"[BPartyListener] Successfully deleted {rowsAffected} old SRI rows.");
                        }
                        else
                        {
                            Console.WriteLine($"[BPartyListener] No old SRI rows found to delete.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting data: {ex.Message}");
            }
        }

        public bool IsMtFullBatchAvailable()
        {
            using (var conn = new MySqlConnection(this.connectionStringMt))
            {
                conn.Open();

                string endTime = this.StartTime.AddSeconds(600).ToString("yyyy-MM-dd HH:mm:ss");

                string query = $@"
            SELECT EXISTS(
                SELECT 1 
                FROM cdr 
                WHERE StartTime > '{endTime}'
            ) AS DataExists";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    var result = Convert.ToInt32(cmd.ExecuteScalar());
                    return result == 1;
                }
            }
        }

        public bool IsSriFullBatchAvailable()
        {
            using (var conn = new MySqlConnection(this.connectionStringSri))
            {
                conn.Open();

                string endTime = this.StartTime.AddSeconds(600).ToString("yyyy-MM-dd HH:mm:ss");

                string query = $@"
            SELECT EXISTS(
                SELECT 1 
                FROM sri 
                WHERE StartTime > '{endTime}'
            ) AS DataExists";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    var result = Convert.ToInt32(cmd.ExecuteScalar());
                    return result == 1;
                }
            }
        }


        private List<string[]> consistantRows(List<string[]> sriRows)
        {
            foreach (var row in sriRows)
            {
                string startTime = row[Fn.StartTime];

                // Convert the StartTime to MySQL format using the ConvertToMySqlFormat function
                try
                {
                    row[Fn.StartTime] = ConvertToMySqlFormat(startTime);
                }
                catch (FormatException ex)
                {
                    // Handle the case where the date format is invalid (optional)
                    Console.WriteLine($"Error parsing StartTime: {ex.Message}");
                }
            }
            return sriRows;
        }
 }
}
