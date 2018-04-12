using System;
using System.Collections.Generic;
using System.Globalization;

namespace MediationModel
{

    public partial class ratetask//change
    {
        public class DisplayClass
        {
            private ratetask _rateTask = null;//change
            private string _countryName = "";
            private long _id = 0;
            private string _prefix = "";
            private string _description = "";
            private string _countrycode = "";
            private string _counctryname = "";
            private DateTime? _effectivedate = null;
            private DateTime? _validbefore = null;
            private double _rate = 0;
            private int _pulse = 0;
            private int _minimumDurationForRoundup=0;
            private float _fixedChargeDurationSec = 0;
            private float _fixedChargeAmount = 0;
            public DisplayClass(ratetask val, Dictionary<string, countrycode> dicCountry)//change
            {
                this._rateTask = val;
                this._id= this._rateTask.id;
                this._prefix= this._rateTask.Prefix;
                this._description= this._rateTask.description;
                this._countrycode= this._rateTask.CountryCode;
                if (this.CountryCode != "" && dicCountry.ContainsKey(this.CountryCode))
                    this._countryName = dicCountry[this.CountryCode].Name;
                var tempDate = new DateTime();
                if (DateTime.TryParseExact(this._rateTask.startdate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate) == true)
                    this._effectivedate = tempDate;
                double.TryParse(this._rateTask.rateamount, out this._rate);//out to this.rate
                this._pulse = Convert.ToInt32(this._rateTask.Resolution);
                this._minimumDurationForRoundup= Convert.ToInt32(this._rateTask.MinDurationSec);
                this._fixedChargeDurationSec= Convert.ToSingle(this._rateTask.SurchargeTime);
                this._fixedChargeAmount= Convert.ToSingle(this._rateTask.SurchargeAmount);
                this._minimumDurationForRoundup= Convert.ToInt32(this._rateTask.MinDurationSec);
                
                if (DateTime.TryParseExact(this._rateTask.enddate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate) == true)
                    this._validbefore = tempDate;
            }
            public long Id => this._id;

            public string Prefix => this._prefix;

            public string Description => this._description;

            public string CountryCode => this._countrycode;

            public string Country => this._countryName;

            public DateTime? EffectiveFrom => this._effectivedate;

            public double Rate => this._rate;

            public int Pulse => this._pulse;

            public int MinimumDurationForRoundup => this._minimumDurationForRoundup;

            public double FixedChargeDurationSec => this._fixedChargeDurationSec;

            public double FixedChargeAmount => this._fixedChargeAmount;

            public DateTime? ValidBefore => this._validbefore;
        }
    }

   


public partial class route
    {
        public class DisplayClass
        {
            private route _route = null;
            public DisplayClass(route val)
            {
                this._route = val;
            }
            public int IdRoute => this._route.idroute;

            public int IdSwitch => this._route.SwitchId;

            public string CommonRoute => this._route.CommonRoute == 1 ? "Yes" : "No";

            public string RouteName => this._route.RouteName;

            public string Status => this._route.Status == 0 ? "Blocked" : (this._route.Status == 1 ? "Unblocked" : "Undefined");

            public int IngressPort => Convert.ToInt32(this._route.field2);

            public int EgressPort => Convert.ToInt32(this._route.field3);

            public int BothwayPort => Convert.ToInt32(this._route.field4);
        }
    }
}
