using TelcobrightMediation;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using LibraryExtensions;
using MediationModel;
using System.Data.Entity;
using ServiceFamilies;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Config;

namespace Jobs
{
    [Export("Job", typeof(ITelcobrightJob))]
        
    public class JobLcr : ITelcobrightJob
    {
        public override string ToString() => this.RuleName;
        public string RuleName => "JobLCR";
        public string HelpText => "LCR Creating Job on a Particular DateTime";
        public int Id => 16;
        public object Execute(ITelcobrightJobInput jobInputData)
        {
            LcrJobInputData lcrJobInputData = (LcrJobInputData) jobInputData;
            MediationContext mediationContext = null;
            CdrJob cdrJob = null;
            string entityConStr =
                ConnectionManager.GetEntityConnectionStringByOperator(lcrJobInputData.Tbc.Telcobrightpartner.CustomerName,
                lcrJobInputData.Tbc);
            using (PartnerEntities partnerContext = new PartnerEntities(entityConStr))
            {
                partnerContext.Database.Connection.Open();
                mediationContext = new MediationContext(lcrJobInputData.Tbc, partnerContext);
                IServiceFamily sf = null;
                mediationContext.MefServiceFamilyContainer.DicExtensions.TryGetValue(ServiceFamilyType.A2Z, out sf);
                if (sf == null)
                {
                    throw new Exception("A2Z Service Family Extension Not Found! " +
                                        lcrJobInputData.TelcobrightJob.JobName);
                }
                //generate txt table
                LcrJobData lData = null;
                List<RouteWiseCdrsCollection> routeWiseCdrCollections =
                    GenerateRouteWiseCdrsCollections(lcrJobInputData.TelcobrightJob, partnerContext, out lData);
                IdCallWiseListOfRouteVsCost IdCallWiseListOfRouteVsCost = new IdCallWiseListOfRouteVsCost();
                foreach (var singleRouteWiseCollection in routeWiseCdrCollections)
                {
                    List<CdrExt> cdrExtsForThisRoute = singleRouteWiseCollection.Cdrs
                        .Select(c => new CdrExt(c,CdrNewOldType.NewCdr)).ToList();
                    CdrCollectionResult collectionResult =
                        new CdrCollectionResult(lcrJobInputData.Ne, cdrExtsForThisRoute,
                                new List<cdrinconsistent>(), cdrExtsForThisRoute.Count,new List<string[]>());
                    CdrJobInputData cdrJobInputData=new CdrJobInputData(mediationContext,partnerContext,
                        lcrJobInputData.Ne,lcrJobInputData.TelcobrightJob);
                    CdrJobContext cdrJobContext=new CdrJobContext(cdrJobInputData,
                        cdrExtsForThisRoute.Select(c=>c.StartTime.Date.RoundDownToHour()).Distinct().ToList());
                    CdrProcessor cdrProcessor=new CdrProcessor(cdrJobContext,collectionResult);
                    cdrJob = new CdrJob(cdrProcessor: cdrProcessor, cdrEraser: null,
                        actualStepsCount: cdrExtsForThisRoute.Count, partialCdrTesterData: null);
                    ExecutePseudoRating(singleRouteWiseCollection.Route,cdrProcessor,sf,IdCallWiseListOfRouteVsCost);
                }
                List<lcr> lstNewLcr = IdCallWiseListOfRouteVsCost.GetLcr(lData);
                //write SQLs, 
                string sql = "";
                using (DbCommand cmd = ConnectionManager.CreateCommandFromDbContext(partnerContext))
                {
                    List<string> lstInsert = lstNewLcr.Where(c => c.LcrCurrent != "")
                        .Select(c => c.GetExtendedInsertSql()).ToList();
                    //fields:prefix,idrateplan,startdate,lcrcurrent
                    cmd.CommandText = "insert into lcr(prefix,idrateplan,startdate,lcrcurrent) values " +
                                      string.Join(",", lstInsert) + ";";
                    cmd.ExecuteNonQuery(); //write lcr done
                    //Cmd.CommandText = "use telcobrightmediation;";//update job table
                    //Cmd.ExecuteNonQuery();
                    sql = " update job set CompletionTime='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'," +
                          " NoOfRecords=" + lstNewLcr.Count + "," +
                          " Status=1 " +
                          " where id=" + lcrJobInputData.TelcobrightJob.id;
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                    string databaseName = lcrJobInputData.Tbc.DatabaseSetting.DatabaseName;
                    cmd.CommandText =
                        "use " + databaseName + ";"; //must change back immediately, even if rollback occurs
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = " commit; ";
                    cmd.ExecuteNonQuery();
                }
                return JobCompletionStatus.Complete;
            }
        }

        public object PreprocessJob(ITelcobrightJobInput jobInputData)
        {
            throw new NotImplementedException();
        }

        public object PostprocessJob(ITelcobrightJobInput jobInputData)
        {
            throw new NotImplementedException();
        }

        private void ExecutePseudoRating(route thisRoute, CdrProcessor cdrProcessor, IServiceFamily sf,
            IdCallWiseListOfRouteVsCost IdCallWiseListOfRouteVsCost)
        {

            //pseudo rating routewise

            //route thisRoute = routeWiseCdr.Route;
            //foreach (cdr thisCdr in routeWiseCdr.Cdrs)
            foreach (CdrExt cdrExt in cdrProcessor.CollectionResult.ConcurrentCdrExts.Values)
            {
                //service group1 (domestic) has been directly referred in this project
                IServiceFamily a2ZServiceFamily=new SfA2Z();
                var serviceGroupConfiguration = new ServiceGroupConfiguration(idServiceGroup: 1);
                ServiceContext serviceContext = new ServiceContext(cdrProcessor, 
                    serviceGroupConfiguration 
                    , a2ZServiceFamily,
                    ServiceAssignmentDirection.Supplier, 0,null);
                Random random = new Random();
                List<string[]> thisRowAsList = new List<string[]>();
                //thisRowAsList.Add(thisRow);

                //todo: will need to adjust eventcdr case for lcr generation
                //look carefully the commented out codes & try to adjust to new object based cdr handling instead of txtrows    
                //new EventVoiceCdr(random.Next().ToString(),
                //thisRow[Fn.StartTime].ConvertToDateTimeFromMySqlFormat(),
                //thisRowAsList, new List<string[]>());
                sf.Execute(cdrExt, serviceContext, true);
                var key = new ValueTuple<int, int, int>(1, 2, 0);
                acc_chargeable chargeable =
                    cdrExt.Chargeables[new ValueTuple<int, int, int>(1, 1, 2)];
                if (chargeable != null) //sg=1,sf=1,assigndir=2, product=0
                {
                    Double cost = Convert.ToDouble(cdrExt.Cdr.OutPartnerCost);
                    //double.TryParse(thisRow[Fn.OutPartnerCost], out cost);
                    RouteVsCost routeVsCost = new RouteVsCost(thisRoute, cost, chargeable.Prefix);
                    IdCallWiseListOfRouteVsCost.Append(cdrExt.Cdr.IdCall, routeVsCost);
                }
                else
                {
                    throw new KeyNotFoundException();
                }
            }
        }

        private List<RouteWiseCdrsCollection> GenerateRouteWiseCdrsCollections(job job, PartnerEntities context, out LcrJobData lData)
        {
            int fieldCount = 0;
            fieldCount = context.cdrfieldlists.Count();
            List<RouteWiseCdrsCollection> routeWiseCdrs = new List<RouteWiseCdrsCollection>();
            lData = JsonConvert.DeserializeObject<LcrJobData>(job.JobParameter);
            DateTime startDate = lData.StartDate;
            List<string> lstPrefix = lData.LstPrefix;
            //take distinct only
            var x = lstPrefix.Select(c => c).Distinct().ToList(); //duplicates removed
            lstPrefix = x;
            //get list of routes that are used in outbound a2z routing
            List<int?> lstTempPartners = context.rateplanassignmenttuples
                .Where(c => c.idService == 1 && c.AssignDirection == 2).Select(c => c.idpartner).ToList();
            List<int> lstIdPartners = new List<int>();
            foreach (int? val in lstTempPartners)
            {
                lstIdPartners.Add(Convert.ToInt32(val));
            }
            List<route> lstRoutes = context.routes.Where(c => lstIdPartners.Contains(c.idPartner))
                .Include("partner").ToList();

            //exclude one or all routes of a supplier based on exclusion flag (field3) in rateassign table
            List<int> idAssignmentTuplesStr = context.rateassigns.Where(c => c.field3 == 1)
                .Select(c => c.Prefix).ToList();
            List<long> idAssignmentTuples = new List<long>();
            idAssignmentTuplesStr.ForEach(c => idAssignmentTuples.Add(Convert.ToInt64(c)));
            List<rateplanassignmenttuple> assignTuples =
                context.rateplanassignmenttuples
                    .Where(c => c.idService == 1 && c.AssignDirection == 2 && idAssignmentTuples.Contains(c.id))
                    .ToList();
            Dictionary<int, route> dicIdWiseRoutes = context.routes.ToDictionary(c => c.idroute);
            Dictionary<int, List<route>> dicPartnerWiseRoutes =
                dicIdWiseRoutes.Values.GroupBy(c => c.idPartner)
                    .ToDictionary(values => values.Key, values => values.ToList());
            List<int> idRoutesToExlude = new List<int>();
            foreach (rateplanassignmenttuple rtp in assignTuples)
            {
                if (rtp.route == null) //exclude all routes for this partner
                {
                    List<route> partnerRoutes = new List<route>();
                    dicPartnerWiseRoutes.TryGetValue(Convert.ToInt32(rtp.idpartner), out partnerRoutes);
                    idRoutesToExlude.AddRange(partnerRoutes.Select(c => c.idroute).ToList());
                }
                else if (rtp.route > 0)
                {
                    idRoutesToExlude.Add(Convert.ToInt32(rtp.route));
                }
            }
            foreach (route r in lstRoutes)
            {
                if (idRoutesToExlude.Contains(r.idroute)) continue;
                routeWiseCdrs.Add(new RouteWiseCdrsCollection(r));
            }

            //gen call record
            foreach (RouteWiseCdrsCollection rw in routeWiseCdrs)
            {
                GenerateCallRecordRoutewise(rw, lstPrefix, fieldCount, startDate);
            }
            return routeWiseCdrs;
        }

        private void GenerateCallRecordRoutewise(RouteWiseCdrsCollection rw, List<string> lstPrefix, int fieldCount, DateTime lcrDate)
        {
            foreach (string prefix in lstPrefix)
            {
                string[] thisRow = new string[fieldCount];
                cdr newCdr=new cdr();
                newCdr.IdCall = Convert.ToInt64(prefix);//use prefix as the IdCall as well, dup ids are filtered already, this is not a real mediation case
                newCdr.TerminatingCalledNumber = prefix;
                newCdr.DurationSec = 60;
                newCdr.OutgoingRoute = rw.Route.RouteName;
                newCdr.SwitchId= rw.Route.SwitchId;
                newCdr.OutPartnerId = rw.Route.idPartner;
                newCdr.AnswerTime = lcrDate;
                newCdr.StartTime = lcrDate;
                newCdr.ChargingStatus = 1;
                rw.Cdrs.Add(newCdr);
            }
        }

    }
}
