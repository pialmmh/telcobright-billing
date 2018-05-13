using System;
using System.Collections.Generic;
using System.Text;

namespace MediationModel
{
    public class Rateext : rate
    {
        public int IdRatePlanAssignmentTuple { get; set; }
        public override string ToString()
        {
            return new StringBuilder().Append(this.Prefix).Append("/")
                .Append(this.id)
                .Append("/")
                .Append(this.P_Startdate != null ? Convert.ToDateTime(this.P_Startdate).ToString("yyyy-MM-dd HH:mm:ss") : "null")
                .Append("/")
                .Append(this.P_Enddate != null ? Convert.ToDateTime(this.P_Enddate).ToString("yyyy-MM-dd HH:mm:ss") : "null").ToString();
        }

        public int? Priority { get; set; }
        public int AssignmentFlag { get; set; }
        private DateTime? Enddatebyrateplan { get; set; }
        private DateTime? Startdatebyrateplan { get; set; }

        private int OpenRateAssignment { get; set; }

        public int IdPartner { get; set; }
        public int IdRoute { get; set; }
        public string TechPrefix { get; set; }
        public string Pcurrency { get; set; }
        public string PrefixWithTechPrefix(ref Dictionary<string, rateplan> dicRatePlan)
        {
            return dicRatePlan[this.idrateplan.ToString()].field4 + this.Prefix;
        }

        public DateTime? P_Startdate
        {
            get
            {
                if (this.AssignmentFlag == 0)
                    return this.startdate;
                return this.startdate >= this.Startdatebyrateplan ? this.startdate : this.Startdatebyrateplan;
            }
        }

        public DateTime? P_Enddate
        {
            get
            {
                if (this.AssignmentFlag == 0)
                    return this.enddate;

                if (this.OpenRateAssignment == 1)//the rate's rateplan assignment is open
                {
                    if (this.enddate == null)
                        return null;
                    else//enddate not null
                        return this.enddate;
                }
                else//rateplan assignment has a an enddate, NOT OPEN
                {
                    if (this.enddate == null)
                        return this.Enddatebyrateplan;
                    else//enddate not null
                        return this.enddate <= this.Enddatebyrateplan ? this.enddate : this.Enddatebyrateplan;
                }
            }
        }
    }
}