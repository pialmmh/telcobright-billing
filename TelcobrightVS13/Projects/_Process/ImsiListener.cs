using LibraryExtensions;
using LibraryExtensions.ConfigHelper;
using MediationModel;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelcobrightMediation;

namespace Process
{
    class ImsiListener
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

        public ImsiListener(TelcobrightConfig tbc)
        {
            this.DatabaseSetting = tbc.DatabaseSetting;
            this.Tbc = tbc;
            this.connectionString = $"Server={this.DatabaseSetting.ServerName};Database=smshub;User ID={this.DatabaseSetting.WriteUserNameForApplication};Password={this.DatabaseSetting.WritePasswordForApplication};SslMode=none;";
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
        /// Fetches all rows from the MEMORY table.
        /// </summary>
        public List<string[]> FetchSriRows()
        {
            List<string[]> rows = new List<string[]>();
            int batchSize = this.Tbc.CdrSetting.SegmentSizeForDbWrite;
            string tableName = "sri";
            DateTime StartTime = this.StartTime;
            DateTime EndTime = this.EndTime;

            int offset = 0;
            bool hasMoreRows = true;

            while (hasMoreRows)
            {
                string query = $@"
            SELECT * FROM {tableName}
            WHERE StartTime >= '{StartTime.ToString("yyyy-MM-dd HH:mm:ss")}'
                AND StartTime <= '{EndTime.ToString("yyyy-MM-dd HH:mm:ss")}'
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

            return rows;
        }
        public List<string[]> FetchCdrRows()
        {
            List<string[]> rows = new List<string[]>();
            int batchSize = this.Tbc.CdrSetting.SegmentSizeForDbWrite;
            string tableName = "cdr";
            DateTime StartTime = this.StartTime;
            DateTime EndTime = this.EndTime;

            int offset = 0;
            bool hasMoreRows = true;

            while (hasMoreRows)
            {
                string query = $@"
            SELECT * FROM {tableName}
            WHERE StartTime >= '{StartTime.ToString("yyyy-MM-dd HH:mm:ss")}'
                AND StartTime <= '{EndTime.ToString("yyyy-MM-dd HH:mm:ss")}'
            LIMIT {batchSize} OFFSET {offset};";

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            List<string> row = new List<string>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row.Add(reader[i].ToString());
                            }
                            rows.Add(row.ToArray());
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

            return rows;
        }

        public List<string[]> aggregateMtWithSri(List<string[]> aggSri, List<string[]> aggMt)
        {

            List<DateTime> dates = aggMt.AsParallel()
                                       .Select(r => r[Fn.StartTime].ConvertToDateTimeFromMySqlFormat())
                                       .ToList();

            // Create a dictionary for fast lookup of aggSri by Imsi
            var sriDictionary = aggSri
                .GroupBy(sri => sri[Fn.Redirectingnumber])
                .ToDictionary(g => g.Key, g => g.ToList());

            // List to track items to remove from aggins
            var itemsToRemove = new HashSet<string>();

            // Process each element in aggMt
            foreach (var mt in aggMt)
            {
                var sriMatches = new List<string[]>();
                if (sriDictionary.TryGetValue(mt[Fn.Redirectingnumber], out sriMatches))
                {
                    foreach (var sri in sriMatches)
                    {
                        DateTime mtTime = Convert.ToDateTime(mt[Fn.StartTime]);
                        DateTime sriTime = Convert.ToDateTime(sri[Fn.StartTime]);

                        if ((mtTime - sriTime).TotalSeconds >= 60)
                        {
                            mt[Fn.TerminatingCalledNumber] = sri[Fn.TerminatingCalledNumber];
                            sri[Fn.Redirectingnumber] = "done";
                            itemsToRemove.Add(sri[Fn.UniqueBillId]);
                            break;
                        }
                    }
                }
            }

            return aggMt;
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
    }
}
