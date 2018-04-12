using System;

namespace TelcobrightMediation
{
	public class PartnersRoute
	{
		string _partnerName;
		int _idPartner;     //bigint not null  
		string _routeName;
		int _switchId;
		string _originatingAnsPrefix;
		string _terminatingAnsPrefix;
		int _partnerType;
		int _customerPrePaid;
		DateTime _lastBilldate;
		int _nationalorInternational;
		int _roamingAns;

		public string PPartnerName { get { return this._partnerName; } set { this._partnerName = value; } }
		public int PIdPartner { get { return this._idPartner; } set { this._idPartner = value; } }
		public string PRouteName { get { return this._routeName; } set { this._routeName = value; } }
		public int PSwitchId { get { return this._switchId; } set { this._switchId = value; } }
		public string POriginatingAnsPrefix { get { return this._originatingAnsPrefix; } set { this._originatingAnsPrefix = value; } }
		public string PTerminatingAnsPrefix { get { return this._terminatingAnsPrefix; } set { this._terminatingAnsPrefix = value; } }
		public int PPartnerType { get { return this._partnerType; } set { this._partnerType = value; } }
		public int PCustomerPrePaid { get { return this._customerPrePaid; } set { this._customerPrePaid = value; } }
		public DateTime PLastBillDate { get { return this._lastBilldate; } set { this._lastBilldate = value; } }
		public int PNationalorInternational { get { return this._nationalorInternational; } set { this._nationalorInternational = value; } }
		public int PRoamingAns { get { return this._roamingAns; } set { this._roamingAns = value; } }

	}

	


	
}
