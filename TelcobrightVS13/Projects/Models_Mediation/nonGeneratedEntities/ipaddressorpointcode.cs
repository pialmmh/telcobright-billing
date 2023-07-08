namespace MediationModel
{
    using System;
    using System.Collections.Generic;

    public partial class ipaddressorpointcode
    {
        public int idroute { get; set; }
        public string RouteName { get; set; }
        public int SwitchId { get; set; }
        public int CommonRoute { get; set; }
        public int idPartner { get; set; }
        public int NationalOrInternational { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public Nullable<System.DateTime> date1 { get; set; }
        public Nullable<int> field1 { get; set; }
        public Nullable<int> field2 { get; set; }
        public Nullable<int> field3 { get; set; }
        public Nullable<int> field4 { get; set; }
        public string field5 { get; set; }
    }
}
