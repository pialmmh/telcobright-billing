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
        private PartnerEntities Context { get; }
        public TelcobrightConfig Tbc { get; }
        public DatabaseSetting DatabaseSetting { get; }
        private readonly string connectionString;
        private string PartialTablePrefix = "zz_zz_partialevent_sri";
        private DateTime StartTime = new DateTime();
        private DateTime EndTime = new DateTime();
        private List<DateTime> daysList = new List<DateTime>();
        public List<string[]> TxtCdrRows { get; set; } = new List<string[]>();
        public Dictionary<DateTime, List<string[]>> DayWiseTxtCdrRows { get; set; } = new Dictionary<DateTime, List<string[]>>();
        public List<string[]> OldRowsToBeDiscardedAfterAggregation { get; } = new List<string[]>();
        //public List<string[]> NewRowsCouldNotBeAggregated { get; set; } = new List<string[]>();
        public List<string[]> OldPartialInstancesFromDB { get; set; } = new List<string[]>();
        public static string InserExpression { get { return " insert into cdr(SwitchId,IdCall,SequenceNumber,FileName,ServiceGroup,IncomingRoute,OriginatingIP,OPC,OriginatingCIC,OriginatingCalledNumber,TerminatingCalledNumber,OriginatingCallingNumber,TerminatingCallingNumber,PrePaid,DurationSec,EndTime,ConnectTime,AnswerTime,ChargingStatus,PDD,CountryCode,AreaCodeOrLata,ReleaseDirection,ReleaseCauseSystem,ReleaseCauseEgress,OutgoingRoute,TerminatingIP,DPC,TerminatingCIC,StartTime,InPartnerId,CustomerRate,OutPartnerId,SupplierRate,MatchedPrefixY,UsdRateY,MatchedPrefixCustomer,MatchedPrefixSupplier,InPartnerCost,OutPartnerCost,CostAnsIn,CostIcxIn,Tax1,IgwRevenueIn,RevenueAnsOut,RevenueIgwOut,RevenueIcxOut,Tax2,XAmount,YAmount,AnsPrefixOrig,AnsIdOrig,AnsPrefixTerm,AnsIdTerm,ValidFlag,PartialFlag,ReleaseCauseIngress,InRoamingOpId,OutRoamingOpId,CalledPartyNOA,CallingPartyNOA,AdditionalSystemCodes,AdditionalPartyNumber,ResellerIds,ZAmount,PreviousRoutes,E1Id,MediaIp1,MediaIp2,MediaIp3,MediaIp4,CallReleaseDuration,E1IdOut,InTrunkAdditionalInfo,OutTrunkAdditionalInfo,InMgwId,OutMgwId,MediationComplete,Codec,ConnectedNumberType,RedirectingNumber,CallForwardOrRoamingType,OtherDate,SummaryMetaTotal,TransactionMetaTotal,ChargeableMetaTotal,ErrorCode,NERSuccess,RoundedDuration,PartialDuration,PartialAnswerTime,PartialEndTime,FinalRecord,Duration1,Duration2,Duration3,Duration4,PreviousPeriodCdr,UniqueBillId,AdditionalMetaData,Category,SubCategory,ChangedByJobId,SignalingStartTime) values "; } }

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
            this.daysList = GetDaysInvolved(this.StartTime, this.EndTime);
            this.DayWiseTxtCdrRows = txtRows
                                            .GroupBy(t => DateTime.ParseExact(t[Fn.StartTime], "yyyy-MM-dd HH:mm:ss", null).Date)
                                            .ToDictionary(d => d.Key, d => d.ToList());
            CreateTable();
        }
        public static List<DateTime> GetDaysInvolved(DateTime startTime, DateTime endTime)
        {
            List<DateTime> daysList = new List<DateTime>();

            // Loop from startTime to endTime, adding each day to the list
            for (DateTime date = startTime.Date; date <= endTime.Date; date = date.AddDays(1))
            {
                daysList.Add(date);
            }
            return daysList;
        }
        /// <summary>
        /// Creates a MEMORY table if it does not exist.
        /// </summary>
        public void CreateTable()
        {
            daysList.ForEach(d =>
            {
                var date = d.ToString("yyyy-MM-dd");
                string tableName = this.PartialTablePrefix + "_"
                                    + date.Replace("-", "");
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
                            PRIMARY KEY (IdCall),
                            KEY  ind_Unique_Bill  ( UniqueBillId ),
                            KEY ind_Start_Time (StartTime)
                        ) ENGINE=MEMORY";

                ExecuteNonQuery(query);
            }
            );

            Console.WriteLine("Table 'PartialEvent' created (if not exists).");
        }

        /// <summary>
        /// Inserts data into the MEMORY table.
        /// </summary>
        public void InsertRow(List<string[]> NewRowsCouldNotBeAggregated)
        {
            int batchSize = this.Tbc.CdrSetting.SegmentSizeForDbWrite;
            if (this.TxtCdrRows.Count == 0) return;

            var DayWiseNewRowsCouldNotBeAggregated = NewRowsCouldNotBeAggregated
                .Select(t =>
                {
                    DateTime parsedDate = DateTime.ParseExact(t[Fn.StartTime], "yyyy-MM-dd HH:mm:ss", null);
                    t[Fn.StartTime] = parsedDate.ToString("yyyy-MM-dd HH:mm:ss");
                    return t;
                })
                .GroupBy(t => DateTime.ParseExact(t[Fn.StartTime], "yyyy-MM-dd HH:mm:ss", null).Date)
                .ToDictionary(d => d.Key, d => d.ToList());



            List<DateTime> daysInvolved = DayWiseNewRowsCouldNotBeAggregated.Keys.ToList();

            using (MySqlConnection con = new MySqlConnection(this.connectionString))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand("", con))
                {
                    foreach (var kv in DayWiseNewRowsCouldNotBeAggregated)
                    {
                        DateTime date = kv.Key;
                        string tableName = this.PartialTablePrefix + "_" + date.ToMySqlFormatDateOnlyWithoutTimeAndQuote().Replace("-", "");

                        // Split rows into batches of size `batchSize`
                        var rows = kv.Value.ToList();
                        int totalRows = rows.Count;
                        for (int i = 0; i < totalRows; i += batchSize)
                        {
                            var batch = rows.Skip(i).Take(batchSize).ToList();  // Get a segment of the rows
                            var stringBuilders = batch.Select(c => GetExtInsertValues(c)).ToList();
                            string InsertExpression = $"insert into {tableName}(IdCall,FileName,TerminatingCalledNumber,EndTime,ChargingStatus,StartTime,PartialFlag,AdditionalSystemCodes,Codec,RedirectingNumber,Duration3,UniqueBillId) values";

                            string sql = InsertExpression + string.Join(",", stringBuilders);
                            sql = sql.EndsWith(";") ? sql : new StringBuilder(sql).Append(";").ToString();
                            cmd.CommandText = sql;
                            cmd.CommandType = CommandType.Text;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                con.Close();
            }

            Console.WriteLine("Rows inserted successfully in batches.");
        }

        /// <summary>
        /// Fetches all rows from the MEMORY table.
        /// </summary>
        public List<string[]> FetchRows()
        {
            List<string[]> rows = new List<string[]>();
            int batchSize = this.Tbc.CdrSetting.SegmentSizeForDbWrite;
            if (this.TxtCdrRows.Count == 0) return new List<string[]>();
            daysList.ForEach(d =>
            {
                var date = d.ToString("yyyy-MM-dd");
                string tableName = this.PartialTablePrefix + "_" + date.Replace("-", "");
                DateTime adjustedStartTime = this.StartTime.AddMinutes(-1);
                DateTime adjustedEndTime = this.EndTime.AddMinutes(1);

                int offset = 0;
                bool hasMoreRows = true;

                while (hasMoreRows)
                {
                    string query = $@"
                SELECT * FROM {tableName}
                WHERE StartTime >= '{adjustedStartTime.ToString("yyyy-MM-dd HH:mm:ss")}'
                  AND StartTime <= '{adjustedEndTime.ToString("yyyy-MM-dd HH:mm:ss")}'
                LIMIT {batchSize} OFFSET {offset};";

                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    string[] row = new string[150];
                                    //for (int i = 0; i < reader.FieldCount; i++)
                                    //{
                                    //    row.Add(reader[i].ToString());
                                    //}
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
                                    rows.Add(row);
                                }
                                // Update offset for the next batch
                                offset += batchSize;
                            }
                            else
                            {
                                hasMoreRows = false;  // Exit loop if no more rows are found
                            }
                        }
                    }
                }
            });

            return rows;
        }

        /// <summary>
        /// Deletes rows from the MEMORY table.
        /// </summary>
        public void DeleteRow()
        {
            //var sriRowIdWithDatesToDelete = this.OldRowsToBeDiscardedAfterAggregation
            //        .Select(s => new KeyValuePair<long, DateTime>(
            //            long.Parse(s[Sn.IdCall]),
            //            DateTime.Parse(s[Sn.StartTime])
            //        ))
            //        .ToList();
            //using (MySqlConnection con = new MySqlConnection(this.connectionString))
            //{
            //    con.Open();
            //    using (MySqlCommand cmd = new MySqlCommand("", con))
            //    {
            //        daysList.ForEach(d =>
            //        {
            //            var date = d.ToString("yyyy-MM-dd");
            //            string tableName = this.PartialTablePrefix + "_"
            //                                + date.Replace("-", "");
            //            int delCount = OldCdrDeleter.DeleteOldCdrs(tableName, sriRowIdWithDatesToDelete,
            //            this.Tbc.CdrSetting.SegmentSizeForDbWrite, cmd);
            //            if (delCount != sriRowIdWithDatesToDelete.Count)
            //                throw new Exception("Deleted number of cdrs do not match raw count in collection result.");
            //        });
            //    }
            //    con.Close();
            //}
            var dayWiseIdCallsToDelete = this.OldRowsToBeDiscardedAfterAggregation
            .GroupBy(s => DateTime.Parse(s[Sn.StartTime]).ToString("yyyy-MM-dd"))
            .ToDictionary(
                g => g.Key,
                g => g.Select(s => long.Parse(s[Sn.IdCall])).ToList()
            );
            foreach (var day in dayWiseIdCallsToDelete)
            {
                string date = day.Key;  // The date (e.g., "2025-02-16")
                var idCalls = day.Value;  // The list of IdCalls for this date

                // Generate the table name using the tablePrefix and the date
                string tableName = this.PartialTablePrefix + "_" + date.Replace("-", "");

                // Process the IdCalls in batches for this particular date's table
                int batchSize = this.Tbc.CdrSetting.SegmentSizeForDbWrite;
                int delCount = 0;
                for (int i = 0; i < idCalls.Count; i += batchSize)
                {
                    var batch = idCalls.Skip(i).Take(batchSize).ToList();

                    // Join IdCall values in the batch to create an IN clause
                    var idCallList = string.Join(",", batch);

                    // Construct the DELETE query for this batch
                    string deleteQuery = $@"
                                        DELETE FROM {tableName} 
                                        WHERE IdCall IN ({idCallList})";
                    using (MySqlConnection con = new MySqlConnection(this.connectionString))
                    {
                        con.Open();
                        // Set the command text and execute the query
                        using (MySqlCommand dbCmd = new MySqlCommand("", con))
                        {
                            dbCmd.CommandText = deleteQuery;
                            delCount += dbCmd.ExecuteNonQuery();
                        }
                    }
                }
                if (delCount != idCalls.Count)
                    throw new Exception("Deleted number of cdrs do not match raw count in collection result.");
            }
        }

        /// <summary>
        /// Utility method for executing SQL commands that don't return data.
        /// </summary>
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
        public StringBuilder GetExtInsertValues(string[] t)
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
                .Append(t[Fn.UniqueBillId].ToMySqlField()).Append(")");
        }
    }
}
