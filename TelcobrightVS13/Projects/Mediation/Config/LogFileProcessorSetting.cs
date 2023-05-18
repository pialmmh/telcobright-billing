using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation.Config
{
    public class LogFileProcessorSetting
    {
        public List<string> BackupSyncPairNames { get; set; }//source side is always vault

        public bool DescendingOrderWhileListingFiles { get; set; }
        public LogFileProcessorSetting()
        {

        }
    }
}
