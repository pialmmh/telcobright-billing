using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using PortalApp.ReportHelper;
using System.Collections.Concurrent;

namespace PortalApp
{
    public class customizeReportHelper
    {
       public static List<SumDataOfMonth> getCustomizeReport(string starDate, string enDate)
       {
           BlockingCollection<SumDataOfMonth> dataStore = new BlockingCollection<SumDataOfMonth>();
            return  new List<SumDataOfMonth>();
        }
    }
}