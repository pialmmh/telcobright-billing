using System;

namespace MediationModel
{
    [Serializable]
    public partial class ansprefixextra { };
    [Serializable]
    public partial class partner
    {
        public override string ToString()
        {
            return this.PartnerName + "/" + this.PartnerType;
        }
    };
    
    [Serializable]
    public partial class partnerbalance { };
    [Serializable]
    public partial class partnercontactmapping { };
    [Serializable]
    public partial class partnerprefix { };
    [Serializable]
    public partial class cdrerror { };
    [Serializable]
    public partial class commontg { };
    [Serializable]
    public partial class countercdrloaded { };
    [Serializable]
    public partial class country { };
    [Serializable]
    public partial class countrycode { };
    [Serializable]
    public partial class enumanstype { };
    [Serializable]
    public partial class enumcalldirection { };
    [Serializable]
    public partial class enumcallforwardingroamingtype { };
    [Serializable]
    public partial class enumpartnertype { };
    [Serializable]
    partial class enumcurrency { };
    [Serializable]
    public partial class enumnationalorinternationalroute { };
    [Serializable]
    public partial class enumpostpaidgenerationstatu { };
    [Serializable]
    public partial class enumpostpaidinvoicestatu { };
    [Serializable]
    public partial class enumprepaidinvoicestatu { };
    [Serializable]
    public partial class enumprepostpaid { };
    [Serializable]
    public partial class enumrateplanformat { };
    [Serializable]
    public partial class enumrateplantype { };
    [Serializable]
    public partial class enumroutestatu { };
    [Serializable]
    public partial class enumservicetype { };
    [Serializable]
    public partial class enumsignalingprotocol { };
    [Serializable]
    public partial class enumss7Networkindicator { };
    [Serializable]
    public partial class enumsubservicetype { };
    [Serializable]
    public partial class enumswitchvendor { };
    [Serializable]
    public partial class enumtelcobrightforpartnertype { };
    [Serializable]
    public partial class enumtransportprotocol { };
    [Serializable]
    partial class rate { };
    [Serializable]
    public partial class rateassign { };
    [Serializable]
    partial class rateplan { };
    [Serializable]
    public partial class rateplanassign { };
    [Serializable]
    partial class rateplanassignmenttuple { };
    [Serializable]
    partial class ratetask { };
    [Serializable]
    public partial class ratetaskassign { };
    [Serializable]
    public partial class ratetaskassignreference { };
    [Serializable]
    public partial class ratetaskreference { };
    [Serializable]
    public partial class service { };
    [Serializable]
    public partial class serviceassignmentbycalldirection { };
    [Serializable]
    public partial class reporttemplate { };
    [Serializable]
    partial class route { };
    [Serializable]
    public partial class routeaddressmapping { };
    [Serializable]
    public partial class timezone { };
    [Serializable]
    public partial class usdexchangerateagainstbdt { };
    [Serializable]
    public partial class usdratetotakabymonth { };
    [Serializable]
    public partial class user { };
    [Serializable]
    public partial class xyzprefix { };
    [Serializable]
    public partial class xyzprefixset { };
    [Serializable]
    public partial class xyzselected { };
    [Serializable]
    public partial class zone { };
    
    [Serializable]
    public partial class cdrpartial { };
    [Serializable]
    public partial class cdrsummary { };
    [Serializable]
    public partial class enumbillingspan { };

    [Serializable]
    public partial class userrole { };

    [Serializable]
    public partial class role { };
}
