using TelcobrightMediation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using TelcobrightFileOperations;
using MediationModel;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Mediation.Cdr;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Text;
using LibraryExtensions;
using TelcobrightInfra.PerformanceAndOptimization;
using TelcobrightMediation.Cdr.Collection.PreProcessors;

namespace Decoders
{

    [Export("Decoder", typeof(AbstractCdrDecoder))]
    public class SigtranDecoder : AbstractCdrDecoder
    {
        public override string ToString() => this.RuleName;
        public override string RuleName => GetType().Name;
        public override int Id =>78;
        public override string HelpText => "Sigtarn Decoder";
        public override CompressionType CompressionType { get; set; }
        //public override string UniqueEventTablePrefix { get; }
        //public override string PartialTableStorageEngine { get; }
        //public override string partialTablePartitionColName { get; }
        protected virtual CdrCollectorInputData Input { get; set; }
     
        public override List<string[]> DecodeFile(CdrCollectorInputData decoderInputData, out List<cdrinconsistent> inconsistentCdrs)
        {
            this.Input = decoderInputData;
            string fileName = this.Input.FullPath;
            Dictionary<string, partnerprefix> ansPrefixes = decoderInputData.MediationContext.AnsPrefixes880;
            inconsistentCdrs = new List<cdrinconsistent>();
            List<string[]> decodeRows = new List<string[]>();
            SigtranPacketDecoder decoder = new SigtranPacketDecoder(fileName, ansPrefixes);
            List<SigtranPacket> packets = decoder.DecodePackets();
            decodeRows = decoder.CdrRecords(packets);
            return decodeRows;
        }
       

        public override string getTupleExpression(Object data)
        {
            Dictionary<string, object> dataAsDic = (Dictionary<string, object>)data;
            CdrCollectorInputData collectorInput = (CdrCollectorInputData)dataAsDic["collectorInput"];
            CdrSetting cdrSetting = collectorInput.CdrSetting;
            string[] row = (string[])dataAsDic["row"];
            int switchId = collectorInput.Ne.idSwitch;
            DateTime startTime = getEventDatetime(new Dictionary<string, object>
            {
                {"cdrSetting", cdrSetting},
                {"row", row}
            });
            //23:00 hours eventid to be rounded up as 00:00 next hour in uniqueEventTupleId                        
            //aggregation logic checks cdr for +-1 hour, so collection and aggregation will be possible            
            if (startTime.Hour == 23)
            {
                startTime = startTime.Date.AddDays(1);
            }
            else
            {
                //startTime = startTime.Date.AddHours(startTime.Hour); prev logic
                startTime = startTime.Date; //new logic
            }
            string sessionId = row[Fn.UniqueBillId];
            string separator = "/";
            return new StringBuilder(switchId.ToString()).Append(separator)
                .Append(startTime.ToMySqlFormatDateOnlyWithoutTimeAndQuote()).Append(separator)
                .Append(sessionId).ToString();
        }


        public override EventAggregationResult Aggregate(object data)
        {

            return SmsHubAggregationHelper.Aggregate((NewAndOldEventsWrapper<string[]>)data);
        }

        public override List<NewAndOldEventsWrapper<string[]>> PreAggregateL1(object data, 
            out List<string[]> failedPreAggCandidates)
        {
            NewCdrPreProcessor preProcessor = (NewCdrPreProcessor) data;
            List<NewAndOldEventsWrapper<string[]>> preAggCandidates =
                separatePreAggVsFailedCandidatesL1(preProcessor.TxtCdrRows,out failedPreAggCandidates);//main preAgg
            return preAggCandidates;
        }

        public override List<NewAndOldEventsWrapper<string[]>> PreAggregateL2(object data,
            out List<NewAndOldEventsWrapper<string[]>> failedPreAggWrappers)
        {
            NewAndOldEventsWrapper<string[]> newAndOldEventsWrapper = (NewAndOldEventsWrapper<string[]>)data;
            List<NewAndOldEventsWrapper<string[]>> preAggCandidates =
                separatePreAggVsFailedCandidatesL2(newAndOldEventsWrapper, out failedPreAggWrappers);//main preAgg
            return  preAggCandidates;
        }

        private List<NewAndOldEventsWrapper<string[]>> separatePreAggVsFailedCandidatesL1(List<string[]> txtCdrRows, 
            out List<string[]> failedRows)
        {
            Dictionary<string, List<string[]>> billIdWiseRows = txtCdrRows
                .GroupBy(r => r[Fn.UniqueBillId])
                .ToDictionary(g => g.Key, g => g.ToList());
            List<NewAndOldEventsWrapper<string[]>> preAggCandidates = new List<NewAndOldEventsWrapper<string[]>>();
            failedRows = new List<string[]>();
            foreach (KeyValuePair<string, List<string[]>> rows in billIdWiseRows)
            {
                NewAndOldEventsWrapper<string[]> row = new NewAndOldEventsWrapper<string[]>
                {
                    UniqueBillId = rows.Key,
                    NewUnAggInstances = rows.Value
                };
                //if (rows.Key == "2024-07-07/28-23/3c0022bc")
                //{
                //    ;
                //}
                //EventAggregationResult 
                List<string[]> failedPreAggCandidates;
               
                List<NewAndOldEventsWrapper<string[]>> aggCandidatesForThisBillId = UngroupReqResL1(row, out failedPreAggCandidates);
                if (rows.Value.Count != aggCandidatesForThisBillId.Count * 2 + failedPreAggCandidates.Count)
                {
                    UngroupReqResL1(row, out failedPreAggCandidates);
                    throw new Exception("ungroupedReqRes must be equal with rows count");
                }
                preAggCandidates.AddRange(aggCandidatesForThisBillId);
                failedRows.AddRange(failedPreAggCandidates);
                //preAggregationResults.Add(rows.Key, aggregationResult);
            }
            return preAggCandidates;
        }

        private List<NewAndOldEventsWrapper<string[]>> separatePreAggVsFailedCandidatesL2(NewAndOldEventsWrapper<string[]> newAndOldEventsWrapper,
            out List<NewAndOldEventsWrapper<string[]>> failedWrappers)
        {
            //Dictionary<string, List<string[]>> billIdWiseRows = txtCdrRows
            //    .GroupBy(r => r[Fn.UniqueBillId])
            //    .ToDictionary(g => g.Key, g => g.ToList());
            List<NewAndOldEventsWrapper<string[]>> preAggCandidates = new List<NewAndOldEventsWrapper<string[]>>();
            failedWrappers = new List<NewAndOldEventsWrapper<string[]>>();
            //EventAggregationResult 
            List<NewAndOldEventsWrapper<string[]>> failedPreAggCandidates;

            List<NewAndOldEventsWrapper<string[]>> aggCandidatesForThisBillId =
                UngroupReqResL2(newAndOldEventsWrapper, out failedPreAggCandidates);
            //if (newAndOldEventsWrapper.Count != aggCandidatesForThisBillId.Count * 2 + failedPreAggCandidates
            //        .SelectMany(n => n.NewUnAggInstances)
            //        .Count() + failedPreAggCandidates
            //        .SelectMany(n => n.OldUnAggInstances)
            //        .Count())
            //{
            //    //UngroupReqRes(row, out failedPreAggCandidates);
            //    throw new Exception("rows count must be equal with temp");
            //}
            preAggCandidates.AddRange(aggCandidatesForThisBillId);
            failedWrappers.AddRange(failedPreAggCandidates);
            //preAggregationResults.Add(rows.Key, aggregationResult);
            return preAggCandidates;
        }

        public static List<NewAndOldEventsWrapper<string[]>> UngroupReqResL1(NewAndOldEventsWrapper<string[]> newAndOldEventsWrapper,
            out List<string[]> failedRows)
        {
            List<NewAndOldEventsWrapper<string[]>> aggCandidates = new List<NewAndOldEventsWrapper<string[]>>();
            failedRows = new List<string[]>();
            List<string[]> newUnAggInstances = newAndOldEventsWrapper.NewUnAggInstances
                .OrderBy(row => row[Sn.StartTime])
                .ToList();
            List<string[]> oldUnAggInstances = newAndOldEventsWrapper.OldUnAggInstances
                .OrderBy(row => row[Sn.StartTime])
                .ToList();
            List<string[]> allUnaggregatedInstances = newUnAggInstances.Concat(oldUnAggInstances)
                .OrderBy(row => row[Sn.StartTime]).ToList();
            //if (allUnaggregatedInstances.Where(r => r[Sn.UniqueBillId] == "2024-09-22/28-23/3b00e3d2").ToList().Count > 1)
            //{
            //    Console.WriteLine("my test again");
            //}
            //if (allUnaggregatedInstances.Any(row => row[Sn.UniqueBillId] == "2024-09-22/28-23/3b00e3d2"))
            //{
            //    ;
            //}
            List<string[]> mtReqs = allUnaggregatedInstances.Where(r => r[Sn.SmsType] == "3")
                .ToList();
            List<string[]> mtResps = allUnaggregatedInstances.Where(r => r[Sn.SmsType] == "4")
                .ToList();

            List<string[]> sriReqs = allUnaggregatedInstances.Where(r => r[Sn.SmsType] == "1")
                .ToList();
            List<string[]> sriResps = allUnaggregatedInstances.Where(r => r[Sn.SmsType] == "2")
                .ToList();
            List<NewAndOldEventsWrapper<string[]>> failedToAggregate;


            var mtWrappers = MatchCandidatesL1(out failedToAggregate, mtReqs, mtResps);
            aggCandidates.AddRange(mtWrappers);
            failedRows.AddRange(failedToAggregate.SelectMany(wr=>wr.NewUnAggInstances));

            var sriWrappers = MatchCandidatesL1(out failedToAggregate, sriReqs, sriResps);
            aggCandidates.AddRange(sriWrappers);
            failedRows.AddRange(failedToAggregate.SelectMany(wr => wr.NewUnAggInstances));

            if (!failedRows.Any() && !aggCandidates.Any())
            {
                //failedRows.AddRange(failedToAggregate.SelectMany(wr => wr.NewUnAggInstances));
                failedRows.AddRange(newUnAggInstances);
            }

            //if (oldUnAggInstances.Any())
            //{
            //    SeperatingOldAndNewInstances(failedRows, oldUnAggInstances);
            //    SeperatingOldAndNewInstances(aggCandidates, oldUnAggInstances);
            //}
            if (aggCandidates.Count * 2 + failedRows.Count != allUnaggregatedInstances.Count)
            {
                throw new Exception("Testing if something is wrong here.");
            }

            return aggCandidates;
        }
        public static List<NewAndOldEventsWrapper<string[]>> UngroupReqResL2(NewAndOldEventsWrapper<string[]> newAndOldEventsWrapper, 
            out List<NewAndOldEventsWrapper<string[]>> cdrsCouldNotBeAggregated)
        {
            //true=new, false=old

            Dictionary<string, bool> idCallWiseNewAndOldFlagsForRow=
                    mapIdCallWiseNewAndOldFlagsForRows(newAndOldEventsWrapper);
            Action<NewAndOldEventsWrapper<string[]>> repositionOldAndNew = wr =>
            {
                wr.OldUnAggInstances = wr.NewUnAggInstances
                    .Where(r => idCallWiseNewAndOldFlagsForRow[r[Sn.IdCall]] == false).ToList();
                wr.NewUnAggInstances = wr.NewUnAggInstances
                    .Where(r => idCallWiseNewAndOldFlagsForRow[r[Sn.IdCall]] == true).ToList();
            };
            List<NewAndOldEventsWrapper<string[]>> aggCandidates = new List<NewAndOldEventsWrapper<string[]>>();
            cdrsCouldNotBeAggregated = new List<NewAndOldEventsWrapper<string[]>>();

            List<string[]> newUnAggInstances = newAndOldEventsWrapper.NewUnAggInstances
                .OrderBy(row => row[Sn.StartTime])
                .ToList();
            List<string[]> oldUnAggInstances = newAndOldEventsWrapper.OldUnAggInstances
                .OrderBy(row => row[Sn.StartTime])
                .ToList();
            List<string[]> allUnaggregatedInstances = newUnAggInstances.Concat(oldUnAggInstances)
                .OrderBy(row => row[Sn.StartTime]).ToList();
            //tmp code
            //if (allUnaggregatedInstances.Where(row => row[Sn.UniqueBillId] == "2024-09-22/28-23/3b00e3d2").ToList()
            //        .Count > 1)
            //{
            //    ;
            //}

            //if (allUnaggregatedInstances.Any(row => row[Sn.UniqueBillId] == "2024-09-22/28-23/3b00e3d2"))
            //{
            //    ;
            //}
            // temp code 
            //if (newUnAggInstances.Any() && oldUnAggInstances.Any())
            //{
            //    List<NewAndOldEventsWrapper<string[]>> tempAgg = new List<NewAndOldEventsWrapper<string[]>>();
            //    List<string[]> mtRq = allUnaggregatedInstances.Where(r => r[Sn.SmsType] == "3")
            //        .ToList();
            //    List<string[]> mtRes = allUnaggregatedInstances.Where(r => r[Sn.SmsType] == "4")
            //        .ToList();
            //    if (mtRq.Any() && mtRes.Any())
            //    {
            //        Console.WriteLine("cornar case");
            //        List<NewAndOldEventsWrapper<string[]>> tmpFailed;
            //        var tmpMtSuccessWrappers = MatchCandidatesL2(out tmpFailed, mtRq, mtRes);
            //        tmpMtSuccessWrappers.ForEach(wr => repositionOldAndNew(wr));
            //        //mtsucc
            //        tempAgg.AddRange(tmpMtSuccessWrappers);
            //    }
            //}

            List<string[]> mtReqs = allUnaggregatedInstances.Where(r => r[Sn.SmsType] == "3")
                .ToList();
            List<string[]> mtResps = allUnaggregatedInstances.Where(r => r[Sn.SmsType] == "4")
                .ToList();

            List<string[]> sriReqs = allUnaggregatedInstances.Where(r => r[Sn.SmsType] == "1")
                .ToList();
            List<string[]> sriResps = allUnaggregatedInstances.Where(r => r[Sn.SmsType] == "2")
                .ToList();
            List<NewAndOldEventsWrapper<string[]>> failedToAggregate;

            
            var mtSuccessWrappers = MatchCandidatesL2(out failedToAggregate, mtReqs, mtResps);
            mtSuccessWrappers.ForEach(wr=>repositionOldAndNew(wr));
            //mtsucc
            aggCandidates.AddRange(mtSuccessWrappers);
            cdrsCouldNotBeAggregated.AddRange(failedToAggregate);

            var sriSuccessWrappers = MatchCandidatesL2(out failedToAggregate, sriReqs, sriResps);
            sriSuccessWrappers.ForEach(wr => repositionOldAndNew(wr));

            aggCandidates.AddRange(sriSuccessWrappers);
            cdrsCouldNotBeAggregated.AddRange(failedToAggregate);

            if (!cdrsCouldNotBeAggregated.Any() && !aggCandidates.Any())
            {
                cdrsCouldNotBeAggregated.Add(newAndOldEventsWrapper);
                cdrsCouldNotBeAggregated.ForEach(wr => repositionOldAndNew(wr));
            }
            cdrsCouldNotBeAggregated.ForEach(wr => repositionOldAndNew(wr));
            //if (oldUnAggInstances.Any())
            //{
            //    SeperatingOldAndNewInstances(cdrsCouldNotBeAggregated, oldUnAggInstances);
            //    SeperatingOldAndNewInstances(aggCandidates, oldUnAggInstances);
            //}
            foreach (var aggCandidate in aggCandidates)
            {
                if (!(aggCandidate.NewUnAggInstances.Count == 1 && aggCandidate.OldUnAggInstances.Count == 1))
                {
                    ;
                }
            }
            if (aggCandidates.Count * 2 + cdrsCouldNotBeAggregated.Count != allUnaggregatedInstances.Count)
            {
                throw new Exception("Testing if something is wrong here.");
            }
            return aggCandidates;
        }

        private static Dictionary<string, bool> mapIdCallWiseNewAndOldFlagsForRows(NewAndOldEventsWrapper<string[]> newAndOldEventsWrapper)
        {
            Dictionary<string, bool> idCallWiseNewAndOldFlagsForRow = new Dictionary<string, bool>();
            foreach (var row in newAndOldEventsWrapper.NewUnAggInstances)
            {
                idCallWiseNewAndOldFlagsForRow.Add(row[Fn.IdCall], true);
            }
            foreach (var row in newAndOldEventsWrapper.OldUnAggInstances)
            {
                idCallWiseNewAndOldFlagsForRow.Add(row[Fn.IdCall], false);
            }
            return idCallWiseNewAndOldFlagsForRow;
        }

      
        private static List<NewAndOldEventsWrapper<string[]>> MatchCandidatesL1(out List<NewAndOldEventsWrapper<string[]>> cdrsCouldNotBeAggregated,
            List<string[]> requests, List<string[]> responses)
        {
            if (requests.Count == 1 && responses.Count == 4)
            {
                ;
            }
            //if (requests.Where(r => r[Sn.UniqueBillId] == "2024-07-07/28-23/3c0022bc").ToList().Count > 1)
            //{
            //    Console.WriteLine("my test again");
            //}
            List<NewAndOldEventsWrapper<string[]>> aggCandidates = new List<NewAndOldEventsWrapper<string[]>>();
            cdrsCouldNotBeAggregated = new List<NewAndOldEventsWrapper<string[]>>();
            //if (requests.Count == 0)
            //{
            //    NewAndOldEventsWrapper<string[]> tmpNonAggCandidate = new NewAndOldEventsWrapper<string[]>
            //    {
            //        UniqueBillId = responses[0][Sn.UniqueBillId],
            //        NewUnAggInstances = responses,
            //    };

            //    cdrsCouldNotBeAggregated.Add(tmpNonAggCandidate);
            //}
            //if (requests.Count < responses.Count && requests.Count>0)
            //{
            //    Console.WriteLine("My Test");
            //}


            // here only aggregated candidates will be listed
            for (int reqIndex = 0; reqIndex < requests.Count; reqIndex++)
            {
                string[] req = requests[reqIndex];

                foreach (string[] resp in responses)
                {
                    if (resp[Sn.UniqueBillId].Contains('_')) continue;
                    DateTime reqDateTime = Convert.ToDateTime(req[Sn.StartTime]);
                    DateTime respDateTime = Convert.ToDateTime(resp[Sn.StartTime]);
                    if (respDateTime >= reqDateTime && (respDateTime - reqDateTime).TotalSeconds <= 30 && resp[Sn.InvokeId] == req[Sn.InvokeId])
                    {
                        string newBillId = new StringBuilder(req[Sn.UniqueBillId]).Append("_").Append(reqIndex.ToString())
                            .ToString();
                        req[Sn.UniqueBillId] = newBillId;
                        resp[Sn.UniqueBillId] = newBillId;
                        NewAndOldEventsWrapper<string[]> tmpAggCandidate = new NewAndOldEventsWrapper<string[]>
                        {
                            UniqueBillId = newBillId,
                            NewUnAggInstances = new List<string[]> { req, resp }
                        };
                        aggCandidates.Add(tmpAggCandidate);
                        break;
                    }
                }
            }

            foreach (var request in requests)
            {
                if (!request[Sn.UniqueBillId].Contains('_'))
                {
                    NewAndOldEventsWrapper<string[]> tmpNonAggCandidate = new NewAndOldEventsWrapper<string[]>
                    {
                        UniqueBillId = request[Sn.UniqueBillId],
                        NewUnAggInstances = new List<string[]> { request }
                    };
                    cdrsCouldNotBeAggregated.Add(tmpNonAggCandidate);
                }
            }

            foreach (var response in responses)
            {
                if (!response[Sn.UniqueBillId].Contains('_'))
                {
                    NewAndOldEventsWrapper<string[]> tmpNonAggCandidate = new NewAndOldEventsWrapper<string[]>
                    {
                        UniqueBillId = response[Sn.UniqueBillId],
                        NewUnAggInstances = new List<string[]> { response }
                    };
                    cdrsCouldNotBeAggregated.Add(tmpNonAggCandidate);
                }
            }
            if (aggCandidates.Count * 2 + cdrsCouldNotBeAggregated.Count != requests.Count + responses.Count)
            {
                throw new Exception("Testing if something is wrong here-1.");
            }
            return aggCandidates;
        }



        private static List<NewAndOldEventsWrapper<string[]>> MatchCandidatesL2(out List<NewAndOldEventsWrapper<string[]>> cdrsCouldNotBeAggregated,
            List<string[]> requests, List<string[]> responses)
        {
            List<NewAndOldEventsWrapper<string[]>> aggCandidates = new List<NewAndOldEventsWrapper<string[]>>();
            cdrsCouldNotBeAggregated = new List<NewAndOldEventsWrapper<string[]>>();

            // here only aggregated candidates will be listed
            for (int reqIndex = 0; reqIndex < requests.Count; reqIndex++)
            {
                string[] req = requests[reqIndex];

                foreach (string[] resp in responses)
                {
                    if (resp[Sn.UniqueBillId].Contains("_222")) continue;
                    DateTime reqDateTime = Convert.ToDateTime(req[Sn.StartTime]);
                    DateTime respDateTime = Convert.ToDateTime(resp[Sn.StartTime]);
                    if (respDateTime >= reqDateTime && (respDateTime - reqDateTime).TotalSeconds <= 30 && resp[Sn.InvokeId] == req[Sn.InvokeId])
                    {
                        string newBillId = new StringBuilder(req[Sn.UniqueBillId]).Append("_222").Append(reqIndex.ToString())
                            .ToString();
                        req[Sn.UniqueBillId] = newBillId;
                        resp[Sn.UniqueBillId] = newBillId;
                        NewAndOldEventsWrapper<string[]> tmpAggCandidate = new NewAndOldEventsWrapper<string[]>
                        {
                            UniqueBillId = newBillId,
                            NewUnAggInstances = new List<string[]> { req, resp }
                        };
                        aggCandidates.Add(tmpAggCandidate);
                        break;
                    }
                }
            }

            foreach (var request in requests)
            {
                if (!request[Sn.UniqueBillId].Contains("_222"))
                {
                    NewAndOldEventsWrapper<string[]> tmpNonAggCandidate = new NewAndOldEventsWrapper<string[]>
                    {
                        UniqueBillId = request[Sn.UniqueBillId],
                        NewUnAggInstances = new List<string[]> { request }
                    };
                    cdrsCouldNotBeAggregated.Add(tmpNonAggCandidate);
                }
            }

            foreach (var response in responses)
            {
                if (!response[Sn.UniqueBillId].Contains("_222"))
                {
                    NewAndOldEventsWrapper<string[]> tmpNonAggCandidate = new NewAndOldEventsWrapper<string[]>
                    {
                        UniqueBillId = response[Sn.UniqueBillId],
                        NewUnAggInstances = new List<string[]> { response }
                    };
                    cdrsCouldNotBeAggregated.Add(tmpNonAggCandidate);
                }
            }

            if (aggCandidates.Count*2 + cdrsCouldNotBeAggregated.Count != requests.Count + responses.Count)
            {
                throw new Exception("Testing if something is wrong here-2.");
            }
            return aggCandidates;
        }

        public override string getCreateTableSqlForPartialEvent(object data)
        {
            return $@"CREATE TABLE if not exists <{this.PartialTablePrefix}> (
                          SwitchId  int(11) NOT NULL,
                          IdCall  bigint(20) NOT NULL,
                          SequenceNumber  bigint(20) DEFAULT NULL,
                          FileName  varchar(100) COLLATE utf8mb4_bin NOT NULL,
                          ServiceGroup  int(11) DEFAULT NULL,
                          IncomingRoute  varchar(100) COLLATE utf8mb4_bin DEFAULT NULL,
                          originatingip  varchar(100) COLLATE utf8mb4_bin DEFAULT NULL,
                          OPC  int(11) DEFAULT NULL,
                          OriginatingCIC  int(11) DEFAULT NULL,
                          OriginatingCalledNumber  varchar(100) COLLATE utf8mb4_bin DEFAULT NULL,
                          TerminatingCalledNumber  varchar(100) COLLATE utf8mb4_bin DEFAULT NULL,
                          OriginatingCallingNumber  varchar(100) COLLATE utf8mb4_bin DEFAULT NULL,
                          TerminatingCallingNumber  varchar(100) COLLATE utf8mb4_bin DEFAULT NULL,
                          PrePaid  int(11) DEFAULT NULL,
                          DurationSec  decimal(20,8) DEFAULT NULL,
                          EndTime  datetime DEFAULT NULL,
                          ConnectTime  datetime DEFAULT NULL,
                          AnswerTime  datetime DEFAULT NULL,
                          ChargingStatus  int(11) DEFAULT NULL,
                          PDD  float DEFAULT NULL,
                          CountryCode  varchar(15) COLLATE utf8mb4_bin DEFAULT NULL,
                          AreaCodeOrLata  varchar(50) COLLATE utf8mb4_bin DEFAULT NULL,
                          ReleaseDirection  int(11) DEFAULT NULL,
                          ReleaseCauseSystem  int(11) DEFAULT NULL,
                          ReleaseCauseEgress  int(11) DEFAULT NULL,
                          OutgoingRoute  varchar(100) COLLATE utf8mb4_bin DEFAULT NULL,
                          terminatingip  varchar(100) COLLATE utf8mb4_bin DEFAULT NULL,
                          DPC  int(11) DEFAULT NULL,
                          TerminatingCIC  int(11) DEFAULT NULL,
                          StartTime  datetime DEFAULT NULL,
                          InPartnerId  int(11) DEFAULT '0',
                          CustomerRate  tinyint(1) unsigned DEFAULT NULL,
                          OutPartnerId  int(11) DEFAULT '0',
                          SupplierRate  decimal(20,8) DEFAULT NULL,
                          MatchedPrefixY  varchar(20) COLLATE utf8mb4_bin DEFAULT NULL,
                          UsdRateY  tinyint(1) unsigned DEFAULT NULL,
                          MatchedPrefixCustomer  varchar(100) COLLATE utf8mb4_bin DEFAULT NULL,
                          MatchedPrefixSupplier  varchar(100) COLLATE utf8mb4_bin DEFAULT NULL,
                          InPartnerCost  tinyint(1) unsigned DEFAULT NULL,
                          OutPartnerCost  tinyint(1) unsigned DEFAULT NULL,
                          CostAnsIn  tinyint(1) unsigned DEFAULT NULL,
                          CostIcxIn  tinyint(1) unsigned DEFAULT NULL,
                          Tax1  tinyint(1) unsigned DEFAULT NULL,
                          IgwRevenueIn  tinyint(1) unsigned DEFAULT NULL,
                          RevenueAnsOut  tinyint(1) unsigned DEFAULT NULL,
                          RevenueIgwOut  tinyint(1) unsigned DEFAULT NULL,
                          RevenueIcxOut  tinyint(1) unsigned DEFAULT NULL,
                          Tax2  tinyint(1) unsigned DEFAULT NULL,
                          XAmount  tinyint(1) unsigned DEFAULT NULL,
                          YAmount  tinyint(1) unsigned DEFAULT NULL,
                          AnsPrefixOrig  varchar(50) COLLATE utf8mb4_bin DEFAULT NULL,
                          AnsIdOrig  int(11) DEFAULT NULL,
                          AnsPrefixTerm  varchar(50) COLLATE utf8mb4_bin DEFAULT NULL,
                          AnsIdTerm  int(11) DEFAULT NULL,
                          ValidFlag  int(11) DEFAULT NULL,
                          PartialFlag  int(11) DEFAULT NULL,
                          ReleaseCauseIngress  int(11) DEFAULT NULL,
                          InRoamingOpId  int(11) DEFAULT NULL,
                          OutRoamingOpId  int(11) DEFAULT NULL,
                          CalledPartyNOA  int(11) DEFAULT NULL,
                          CallingPartyNOA  int(11) DEFAULT NULL,
                          AdditionalSystemCodes  varchar(100) COLLATE utf8mb4_bin DEFAULT NULL,
                          AdditionalPartyNumber  varchar(100) COLLATE utf8mb4_bin DEFAULT NULL,
                          ResellerIds  varchar(100) COLLATE utf8mb4_bin DEFAULT NULL,
                          ZAmount  tinyint(1) unsigned DEFAULT NULL,
                          PreviousRoutes  varchar(500) COLLATE utf8mb4_bin DEFAULT NULL,
                          E1Id  int(11) DEFAULT NULL,
                          MediaIp1  varchar(100) COLLATE utf8mb4_bin DEFAULT NULL,
                          MediaIp2  varchar(100) COLLATE utf8mb4_bin DEFAULT NULL,
                          MediaIp3  varchar(100) COLLATE utf8mb4_bin DEFAULT NULL,
                          MediaIp4  varchar(100) COLLATE utf8mb4_bin DEFAULT NULL,
                          CallReleaseDuration  float DEFAULT NULL,
                          E1IdOut  int(11) DEFAULT NULL,
                          InTrunkAdditionalInfo  varchar(30) COLLATE utf8mb4_bin DEFAULT NULL,
                          OutTrunkAdditionalInfo  varchar(30) COLLATE utf8mb4_bin DEFAULT NULL,
                          InMgwId  varchar(20) COLLATE utf8mb4_bin DEFAULT NULL,
                          OutMgwId  varchar(20) COLLATE utf8mb4_bin DEFAULT NULL,
                          MediationComplete  int(11) DEFAULT '0',
                          Codec  varchar(20) COLLATE utf8mb4_bin DEFAULT NULL,
                          ConnectedNumberType  int(11) DEFAULT NULL,
                          RedirectingNumber  varchar(30) COLLATE utf8mb4_bin DEFAULT NULL,
                          CallForwardOrRoamingType  int(11) DEFAULT NULL,
                          OtherDate  datetime DEFAULT NULL,
                          SummaryMetaTotal  tinyint(1) unsigned DEFAULT NULL,
                          TransactionMetaTotal  tinyint(1) unsigned DEFAULT NULL,
                          ChargeableMetaTotal  tinyint(1) unsigned DEFAULT NULL,
                          ErrorCode  varchar(200) COLLATE utf8mb4_bin DEFAULT NULL,
                          NERSuccess  int(11) DEFAULT NULL,
                          RoundedDuration  tinyint(1) unsigned DEFAULT NULL,
                          PartialDuration  tinyint(1) unsigned DEFAULT NULL,
                          PartialAnswerTime  datetime DEFAULT NULL,
                          PartialEndTime  datetime DEFAULT NULL,
                          FinalRecord  bigint(20) DEFAULT NULL,
                          Duration1  tinyint(1) unsigned DEFAULT NULL,
                          Duration2  tinyint(1) unsigned DEFAULT NULL,
                          Duration3  tinyint(1) unsigned DEFAULT NULL,
                          Duration4  tinyint(1) unsigned DEFAULT NULL,
                          PreviousPeriodCdr  int(11) DEFAULT NULL,
                          UniqueBillId  varchar(100) COLLATE utf8mb4_bin DEFAULT NULL,
                          AdditionalMetaData  varchar(500) COLLATE utf8mb4_bin DEFAULT NULL,
                          Category  int(11) DEFAULT NULL,
                          SubCategory  int(11) DEFAULT NULL,
                          ChangedByJobId  bigint(20) DEFAULT NULL,
                          SignalingStartTime  datetime DEFAULT NULL,
                          KEY  ind_Unique_Bill  ( UniqueBillId ),
                          KEY ind_Start_Time (StartTime)
                        ) ENGINE=InnoDB ";
        }
        //public string getCreateTableSqlForUniqueEvent(CdrCollectorInputData decoderInputData)
        //{
        //    throw new NotImplementedException();
        //}

        //public string getSelectExpressionForUniqueEvent(CdrCollectorInputData decoderInputData)
        //{
        //    throw new NotImplementedException();
        //}

        //public string getWhereForHourWiseUniqueEventCollection(CdrCollectorInputData decoderInputData, DateTime hourOfDay)
        //{
        //    throw new NotImplementedException();
        //}

        //public string getSelectExpressionForPartialCollection(CdrCollectorInputData decoderInputData)
        //{
        //    throw new NotImplementedException();
        //}

        //public string getWhereForHourWisePartialCollection(CdrCollectorInputData decoderInputData, DateTime hourOfDay)
        //{
        //    throw new NotImplementedException();
        //}

        //public override string getCreateTableSqlForUniqueEvent(object data)
        //{
        //    throw new NotImplementedException();
        //}

        //public override string getSelectExpressionForUniqueEvent(object data)
        //{
        //    throw new NotImplementedException();
        //}

        //public override string getWhereForHourWiseCollection(Object data)
        //{
        //    throw new NotImplementedException();
        //}

        //public override string getSelectExpressionForPartialCollection(Object data)
        //{
        //    throw new NotImplementedException();
        //}

        //public override DateTime getEventDatetime(Object data)
        //{
        //    throw new NotImplementedException();
        //}


    }
}


