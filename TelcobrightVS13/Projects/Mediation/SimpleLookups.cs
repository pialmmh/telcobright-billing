using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Globalization;
using MediationModel;
using MySql.Data.MySqlClient;
using TelcobrightMediation.Config;

namespace TelcobrightMediation
{
    public class SwitchWiseLookup
    {
        public Dictionary<string, causecode> DicCauseCodeCcr { get; set; }
        public Dictionary<string, PartnersRoute> DictRoutes { get; private set; }
        public int IdSwitch { get; }
        private readonly MySqlConnection _conPartner;
        private PartnerEntities Context { get; }
        public SwitchWiseLookup(PartnerEntities context,int idSwitch)
        {
            this.Context = context;
            this.IdSwitch = idSwitch;
            this.DictRoutes = GetPartnersDict(idSwitch);
            this.DicCauseCodeCcr = new Dictionary<string, causecode>();
            List<causecode> lstCauseCodes = context.causecodes.Where(c => c.idSwitch == idSwitch
                                                                          && c.CallCompleteIndicator == 1).ToList();
            foreach (causecode thisCauseCode in lstCauseCodes)
            {
                this.DicCauseCodeCcr.Add(thisCauseCode.idSwitch.ToString() + "-" + thisCauseCode.CC.ToString(),
                    thisCauseCode);
            }
            this.DictRoutes = GetPartnersDict(this.IdSwitch);
        }

        private Dictionary<string, PartnersRoute> GetPartnersDict(int thisSwitchId)
        {
            //main partner dictionary with TrunkGroupName as Key
            Dictionary<string, PartnersRoute> dictPartner = new Dictionary<string, PartnersRoute>();
            string sql = " select c.PartnerName,c.idpartner,r.routename,r.switchid, " +
                         " c.PartnerType,c.CustomerPrePaid,c.Date1 as LastBillDate,  " +
                         " r.NationalorInternational,ifnull(r.field1,-1) as field1 " +//field1=RoamingANS
                         " from partner c " + //can't use left join here as one partner may not yet have route
                         " join route r  " +
                         " on r.idpartner=c.idpartner " +
                         " where r.switchid=" + thisSwitchId;
            using (DbCommand command = ConnectionManager.CreateCommandFromDbContext(this.Context,sql))
            {
                command.CommandType = CommandType.Text;
                DbDataReader dbReader;
                dbReader = command.ExecuteReader();
                while (dbReader.Read())
                {
                    PartnersRoute thisRoute = new PartnersRoute();
                    thisRoute.PPartnerName = dbReader[0].ToString();
                    thisRoute.PIdPartner = int.Parse(dbReader[1].ToString());
                    if (dbReader[2].ToString() != "") thisRoute.PRouteName = dbReader[2].ToString();
                    if (dbReader[3].ToString() != "") thisRoute.PSwitchId = int.Parse(dbReader[3].ToString());
                    thisRoute.PPartnerType = int.Parse(dbReader[4].ToString());
                    thisRoute.PCustomerPrePaid = int.Parse(dbReader[5].ToString());
                    if (dbReader[7].ToString() != "") thisRoute.PNationalorInternational = int.Parse(dbReader[7].ToString());
                    if (int.Parse(dbReader[8].ToString()) != -1) thisRoute.PRoamingAns = int.Parse(dbReader[8].ToString());
                    DateTime tempDate;
                    if (DateTime.TryParseExact(dbReader[6].ToString(), "yyyy-dd-MM", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out tempDate))
                    {
                        thisRoute.PLastBillDate = tempDate;
                    }
                    //use default date to "1800-01-01" if this partner has not been billed before
                    else if (DateTime.TryParseExact("1800-01-01", "yyyy-dd-MM", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out tempDate))
                    {
                        thisRoute.PLastBillDate = tempDate;
                    }
                    //ANS can have routes as well, displayed as roaming routes, so add all the routes
                    //regardless of igw/icx
                    if (thisRoute.PRouteName != null)
                    {
                        dictPartner.Add(thisRoute.PRouteName, thisRoute);
                    }
                }//while
                dbReader.Close();
                dbReader.Dispose();
            }//using mysql command

            return dictPartner;
        }
        
    }

}
