using TelcobrightMediation;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Linq;
using MediationModel;

namespace LcrJob
{

    [Export("AutoCreateJob", typeof(IAutoCreateJob))]
    public class JcLcr : IAutoCreateJob//must change class name here
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        private string _helpText = "Create Job Least Cost Routing (LCR)";//change for each extension
        public string RuleName
        {
            get { return "Create LCR"; }//change for each extension
        }
        public string HelpText
        {
            get { return this._helpText; }
        }
       
        public int Id
        {
            get { return 15; }//change for each extension
        }
       
        public void Execute(ne thisSwitch)
        {

            //TelcobrightConfig tbc = ConfigFactory.GetConfigFromFile(string.Join(Path.DirectorySeparatorChar.ToString(), (System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)).Split(Path.DirectorySeparatorChar).Where(c => c.Contains("file") == false)) + Path.DirectorySeparatorChar.ToString() + "Telcobright.conf");
            //if (tbc.conPartner.State != System.Data.ConnectionState.Open) tbc.conPartner.Open();
            using (MySqlConnection conPartner= new MySqlConnection(ConfigurationManager.ConnectionStrings["Partner"].ConnectionString))
            {
                using (PartnerEntities contextParter = new PartnerEntities())
                {
                    //First check whether uninitialized rate plans exist
                    bool initNewLcr =
                        contextParter.lcrpoints.Any(c => c.RatePlanAssignmentFlag == 1 && c.JobCreated != 1)
                        || contextParter.lcrrateplans.Any(c => c.JobCreated != 1);

                    if (initNewLcr == true)
                    {
                        InitNewLcrRatePlan(thisSwitch, -1,conPartner);
                    }

                    bool incrementalExists = contextParter.lcrpoints.Any(c => c.JobCreated != 1 && c.RatePlanAssignmentFlag != 1);
                    if (incrementalExists == true)
                    {

                    }
                }
            }

        }//execute
        protected class PrefixVsAffected
        {
            public string ChangedPrefixSupplier { get; set; }
            public List<string> LstAffectedPrefixes { get; set; }
            public PrefixVsAffected()
            {
                this.LstAffectedPrefixes = new List<string>();
            }
        }
        protected class LcrIncrementalDateWiseData
        {
            public DateTime TriggerTime { get; set; }
            List<PrefixVsAffected> LstPrefixVsAffecteds { get; set; }
            public LcrIncrementalDateWiseData()
            {
                this.LstPrefixVsAffecteds = new List<PrefixVsAffected>();
            }
        }
        protected class RatePlanVsPrefixes
        {
            public int IdRatePlan { get; set; }
            public List<string> LstPrefixes { get; set; }
            public Dictionary<string, PrefixVsAffected> DicPrefixVsAffected = new Dictionary<string, PrefixVsAffected>();
            public RatePlanVsPrefixes(int id)
            {
                this.LstPrefixes = new List<string>();
            }
            public void PopulatePrefixVsAffected()
            {

            }
        }
        private void CreateIncrementalLcrJobs()
        {
            Dictionary<string, LcrIncrementalDateWiseData> dicIncrementals = new Dictionary<string, LcrIncrementalDateWiseData>();
            using (PartnerEntities context = new PartnerEntities())
            {
                //retrieve reference Rateplan and prefixes within
                List<int> lstIdRateplan = context.lcrrateplans.Select(c => c.idRatePlan).Distinct().ToList();
                List<RatePlanVsPrefixes> lstRefPlaNvsPrefix = new List<RatePlanVsPrefixes>();
                foreach (int idR in lstIdRateplan)
                {
                    RatePlanVsPrefixes onePlanVsPrefixes = new RatePlanVsPrefixes(idR);
                    onePlanVsPrefixes.LstPrefixes = context.rates.Where(c => c.idrateplan == idR).Select(c => c.Prefix).Distinct().ToList();
                    lstRefPlaNvsPrefix.Add(onePlanVsPrefixes);
                }
                //retrieve unattended entries in lcrpoint table, group by doesn't help
                List<lcrpoint> lstLcrPoint = context.lcrpoints.Where(c => c.JobCreated != 1 && c.RatePlanAssignmentFlag == 0).ToList();
                
                //take care of LCR by each rate plan, containing prefixes
                foreach (RatePlanVsPrefixes rvs in lstRefPlaNvsPrefix)
                {

                }
                
            }
        }
        private job CreateOneLcrJob(long idRatePlan, int jobPriority, LcrJobData lcrdata, ne thisSwitch)
        {
            job newJob = new job();
            string jobname = "LCR/RP:" + idRatePlan + "/" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            newJob.JobName = jobname;
            newJob.idjobdefinition = 16;//LCR creation job itself is 15
            newJob.priority = jobPriority;
            newJob.Status = 6;//created
            newJob.JobParameter = JsonConvert.SerializeObject(lcrdata);
            newJob.idNE = thisSwitch.idSwitch;
            newJob.CreationTime = DateTime.Now;
            newJob.SerialNumber = 0;
            return newJob;
        }
        public string InitNewLcrRatePlan(ne thisSwitch, long idLcrRatePlan,MySqlConnection conPartner)
        {
            string jobname = "";
            UpdateLcrRatePlanForNewRatePlanAssignment(conPartner);
            using (PartnerEntities context = new PartnerEntities(conPartner,false))
            {
                List<lcrrateplan> pendingLcRs = new List<lcrrateplan>();
                if (idLcrRatePlan == -1)
                {
                    pendingLcRs = context.lcrrateplans.Where(c => c.JobCreated != 1).ToList();
                }
                else
                {
                    pendingLcRs = context.lcrrateplans.Where(c => c.id == idLcrRatePlan).ToList();
                }
                int jobPriority = context.enumjobdefinitions.Where(c => c.id == 15).Select(c => c.Priority).First();
                foreach (lcrrateplan lp in pendingLcRs)
                {
                    List<string> lstPrefixes = context.rates.Where(c => c.idrateplan == lp.idRatePlan)
                        .Select(c => c.Prefix).ToList();
                    LcrJobData lcrdata = new LcrJobData(lp.idRatePlan);
                    lcrdata.StartDate = lp.StartDate;
                    foreach (string prefix in lstPrefixes)
                    {
                        lcrdata.LstPrefix.Add(prefix);
                    }
                    //save job
                    context.jobs.Add(CreateOneLcrJob(lcrdata.RefRatePlan, jobPriority, lcrdata, thisSwitch));
                    //later wrap in a transaction with sql, can't trust ef
                    context.Database.ExecuteSqlCommand("update lcrrateplan set jobcreated=1 where id=" + lp.id);
                } //for each rateplan
                context.SaveChanges();
            }
            return jobname;
        }


       

        private class IdVsMaxDate
        {
            public int IdRatePlan { get; set; }
            public DateTime MaxDate { get; set; }
        }
        private void UpdateLcrRatePlanForNewRatePlanAssignment(MySqlConnection conPartner)
        {
            using (PartnerEntities ConPartner = new PartnerEntities(conPartner, false))
            {
                //List<int> lstDistinctidRatePlanLcr = ConPartner.lcrrateplans.Select(c => c.idRatePlan).Distinct().ToList();
                List<IdVsMaxDate> lstIdVsMaxDate = ConPartner.Database.SqlQuery<IdVsMaxDate>
                    ("select idrateplan as idRatePlan, max(startdate) as MaxDate from lcrrateplan group by idrateplan").ToList();
                List<string> lstInsertSql = new List<string>();
                Dictionary<string, rateplan> dicRatePlans = ConPartner.rateplans.ToDictionary(c => c.id.ToString());
                List<lcrpoint> lstLcrPoint = ConPartner.lcrpoints.Where(c => c.RatePlanAssignmentFlag == 1 && c.JobCreated != 1).ToList();
                //if there is any entry in lcrpoint due to rate assignment, just re-generate new instance of lcr for each rateplan
                //and then mark all lcrpoint entries as job created
                foreach (IdVsMaxDate maxRp in lstIdVsMaxDate)
                {
                    //regenerate full lcr for each lcrrateplan in a time later than their latest instance in lcrrateplan table
                    //this should cover insert, update, delete in rateassignment table
                    string ratePlanName = dicRatePlans[maxRp.IdRatePlan.ToString()].RatePlanName;
                    DateTime newDate = maxRp.MaxDate.AddSeconds(1);
                    lstInsertSql.Add(" (" + maxRp.IdRatePlan + ",'" + ratePlanName + "/" + newDate.ToString("yyyy-MM-dd HH:mm:ss") + "','" +
                        newDate.ToString("yyyy-MM-dd HH:mm:ss") + "',0)");
                }
                
                
                    using (MySqlCommand cmd = new MySqlCommand("", conPartner))
                    {
                        try
                        {
                            if (lstInsertSql.Count > 0)
                            {
                                cmd.CommandText = " set autocommit=0; ";
                                cmd.ExecuteNonQuery();

                                cmd.CommandText = " insert into lcrrateplan (idrateplan, description,startdate,jobcreated) values "
                                + string.Join(",", lstInsertSql);
                                cmd.ExecuteNonQuery();

                                //set job created flag in lcrrate
                                if (lstLcrPoint.Count > 0)
                                {
                                    cmd.CommandText = " update lcrpoint set jobcreated=1 where id in(" + string.Join(",", lstLcrPoint.Select(c => c.id.ToString())) + ");";
                                    cmd.ExecuteNonQuery();
                                }

                                cmd.CommandText = " commit; ";
                                cmd.ExecuteNonQuery();
                            }

                        }
                        catch (Exception e1)
                        {
                            cmd.CommandText = " rollback; ";
                            cmd.ExecuteNonQuery();
                            throw new Exception(e1.Message + " " + (e1.InnerException != null ? e1.InnerException.ToString() : ""));
                        }
                    }
                
            }
        }
       


    }
}
