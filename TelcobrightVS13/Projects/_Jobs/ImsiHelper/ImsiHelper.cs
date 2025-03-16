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
    class ImsiHelper
    {
        private PartnerEntities Context { get; }
        public TelcobrightConfig Tbc { get; }
        public DatabaseSetting DatabaseSetting { get; }
        private readonly string connectionString;
        private string PartialTablePrefix = "zz_zz_partialevent_sri_";
        private DateTime StartTime = new DateTime();
        private DateTime EndTime = new DateTime();
        private List<DateTime> daysList = new List<DateTime>();
        public List<string[]> TxtCdrRows { get; set; } = new List<string[]>();
        public Dictionary<DateTime, List<string[]>> DayWiseTxtCdrRows { get; set; } = new Dictionary<DateTime, List<string[]>>();
        public List<string[]> OldRowsToBeDiscarded { get; } = new List<string[]>();
        public List<string[]> OldPartialInstancesFromDB { get; set; } = new List<string[]>();
        public static string InserExpression { get { return " insert into cdr(SwitchId,IdCall,SequenceNumber,FileName,ServiceGroup,IncomingRoute,OriginatingIP,OPC,OriginatingCIC,OriginatingCalledNumber,TerminatingCalledNumber,OriginatingCallingNumber,TerminatingCallingNumber,PrePaid,DurationSec,EndTime,ConnectTime,AnswerTime,ChargingStatus,PDD,CountryCode,AreaCodeOrLata,ReleaseDirection,ReleaseCauseSystem,ReleaseCauseEgress,OutgoingRoute,TerminatingIP,DPC,TerminatingCIC,StartTime,InPartnerId,CustomerRate,OutPartnerId,SupplierRate,MatchedPrefixY,UsdRateY,MatchedPrefixCustomer,MatchedPrefixSupplier,InPartnerCost,OutPartnerCost,CostAnsIn,CostIcxIn,Tax1,IgwRevenueIn,RevenueAnsOut,RevenueIgwOut,RevenueIcxOut,Tax2,XAmount,YAmount,AnsPrefixOrig,AnsIdOrig,AnsPrefixTerm,AnsIdTerm,ValidFlag,PartialFlag,ReleaseCauseIngress,InRoamingOpId,OutRoamingOpId,CalledPartyNOA,CallingPartyNOA,AdditionalSystemCodes,AdditionalPartyNumber,ResellerIds,ZAmount,PreviousRoutes,E1Id,MediaIp1,MediaIp2,MediaIp3,MediaIp4,CallReleaseDuration,E1IdOut,InTrunkAdditionalInfo,OutTrunkAdditionalInfo,InMgwId,OutMgwId,MediationComplete,Codec,ConnectedNumberType,RedirectingNumber,CallForwardOrRoamingType,OtherDate,SummaryMetaTotal,TransactionMetaTotal,ChargeableMetaTotal,ErrorCode,NERSuccess,RoundedDuration,PartialDuration,PartialAnswerTime,PartialEndTime,FinalRecord,Duration1,Duration2,Duration3,Duration4,PreviousPeriodCdr,UniqueBillId,AdditionalMetaData,Category,SubCategory,ChangedByJobId,SignalingStartTime) values "; } }

        public ImsiHelper(TelcobrightConfig tbc)
        {
            this.DatabaseSetting = tbc.DatabaseSetting;
            this.Tbc = tbc;
            this.connectionString = $"Server={this.DatabaseSetting.ServerName};Database=smshub;User ID={this.DatabaseSetting.WriteUserNameForApplication};Password={this.DatabaseSetting.WritePasswordForApplication};SslMode=none;";
        }

        /// <summary>
        /// Inserts data into the MEMORY table.
        /// </summary>
        public void InsertRow(List<string[]> aggSriRows)
        {
            int batchSize = this.Tbc.CdrSetting.SegmentSizeForDbWrite;
            var DayWiseNewRowsAggregated = aggSriRows
                .Select(t =>
                {
                    DateTime parsedDate = DateTime.ParseExact(t[Fn.StartTime], "yyyy-MM-dd HH:mm:ss", null);
                    t[Fn.StartTime] = parsedDate.ToString("yyyy-MM-dd HH:mm:ss");
                    return t;
                })
                .GroupBy(t => DateTime.ParseExact(t[Fn.StartTime], "yyyy-MM-dd HH:mm:ss", null).Date)
                .ToDictionary(d => d.Key, d => d.ToList());

            List<DateTime> daysInvolved = DayWiseNewRowsAggregated.Keys.ToList();
            using (MySqlConnection con = new MySqlConnection(this.connectionString))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand("", con))
                {
                    foreach (var kv in DayWiseNewRowsAggregated)
                    {
                        string tableName = "sri";
                        var stringBuilders = kv.Value.ToList().Select(c => GetExtInsertValues(c)).ToList();
                        string InsertExpression = $" insert into {tableName} (TerminatingCalledNumber,StartTime,RedirectingNumber) values";

                        // Segment the insert into batches
                        int totalRows = stringBuilders.Count;
                        int numberOfBatches = (int)Math.Ceiling((double)totalRows / batchSize);

                        for (int batch = 0; batch < numberOfBatches; batch++)
                        {
                            int skip = batch * batchSize;
                            var batchRows = stringBuilders.Skip(skip).Take(batchSize);

                            string sql = InsertExpression + string.Join(",", batchRows);
                            sql = sql.EndsWith(";") ? sql : new StringBuilder(sql).Append(";").ToString();

                            cmd.CommandText = sql;
                            cmd.CommandType = CommandType.Text;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                con.Close();
            }
            Console.WriteLine("Rows inserted successfully.");
        }

        /// <summary>
        /// Fetches all rows from the MEMORY table.
        /// </summary>
        public List<string[]> FetchRows(DateTime startTime, DateTime endTime)
        {
            List<string[]> rows = new List<string[]>();
            int batchSize = this.Tbc.CdrSetting.SegmentSizeForDbWrite;

            daysList.ForEach(d =>
            {
                DateTime adjustedStartTime = startTime.AddMinutes(-1);
                DateTime adjustedEndTime = endTime.AddMinutes(1);

                int offset = 0;
                bool hasMoreRows = true;

                while (hasMoreRows)
                {
                    string query = $@"
                SELECT * FROM sri
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
                                    row[Fn.TerminatingCalledNumber] = reader[0].ToString();
                                    row[Fn.StartTime] = reader[1].ToString();
                                    row[Fn.Redirectingnumber] = reader[2].ToString();
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
        //public void DeleteRow(List<KeyValuePair<long, DateTime>> sriRowIdWithDatesToDelete)
        //{
        //    using (MySqlConnection con = new MySqlConnection(this.connectionString))
        //    {
        //        con.Open();
        //        using (MySqlCommand cmd = new MySqlCommand("", con))
        //        {
        //            daysList.ForEach(d =>
        //            {
        //                int delCount = OldCdrDeleter.DeleteOldCdrs("sri", sriRowIdWithDatesToDelete,
        //                this.Tbc.CdrSetting.SegmentSizeForDbWrite, cmd);
        //                if (delCount != sriRowIdWithDatesToDelete.Count)
        //                    throw new Exception("Deleted number of cdrs do not match raw count in collection result.");
        //            });
        //        }
        //        con.Close();
        //    }
        //}

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
            if(t[Fn.TerminatingCalledNumber].IsNullOrEmptyOrWhiteSpace())
            {
                ;
            }
            return new StringBuilder("(")
                .Append(t[Fn.TerminatingCalledNumber].ToMySqlField()).Append(",")
                .Append(t[Fn.StartTime].ToMySqlField()).Append(",")
                .Append(t[Fn.Redirectingnumber].ToMySqlField()).Append(")");
        }
    }
}
