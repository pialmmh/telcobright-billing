using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PortalApp._portalHelper
{
    public class CDRFileComparer
    {
        public string switchName { get; set; }
        public string FileName { get; set; }
        public int RecordCountFromICX { get; set; }
        public int RecordCountFromTB { get; set; }
        public int DiffRecordCount { get; set; }
        public Decimal ActualDurationFromICX { get; set; }
        public Decimal ActualDuratinoTB { get; set; }
        public Decimal DiffDuration { get; set; }

        public CDRFileComparer(string[] row)
        {
            this.switchName = row[0];
            this.FileName = row[1];
            this.RecordCountFromICX = int.Parse(row[2]);            
            this.ActualDurationFromICX = Decimal.Parse(row[3]);
        }

    }
}