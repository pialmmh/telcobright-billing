using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PortalApp._portalHelper
{
    public class CDRFileComparer
    {
        //public string switchName { get; set; }
        public string FileName { get; set; }
        public string RecordCountFromICX { get; set; }
        public string RecordCountFromTB { get; set; }
        public string DiffRecordCount { get; set; }
        public string ActualDurationFromICX { get; set; }
        public string ActualDuratinoTB { get; set; }
        public string DiffDuration { get; set; }

        public CDRFileComparer(string[] row)
        {
            //this.switchName = row[0].Trim();
            this.FileName = row[0].Trim();
            this.RecordCountFromICX = row[1].Trim();
            this.ActualDurationFromICX = row[2].Trim();
        }

    }
}