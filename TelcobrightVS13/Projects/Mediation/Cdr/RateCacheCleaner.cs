using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation.Cdr
{
    public static class RateCacheCleaner
    {
        public static bool CheckAndClearRateCache(MediationContext mediationContext, Exception e)
        {
            bool cacheLimitExceeded = false;
            try
            {
                if (e.Message.Contains("OutOfMemory")) //ratecache too big and exceeds c#'s limit
                {
                    mediationContext.MefServiceFamilyContainer.RateCache
                        .ClearRateCache(); //involves GC as well to freeup memory instantly
                    cacheLimitExceeded = true;
                }
            }
            catch (Exception e1)
            {
                Console.WriteLine(e);
                throw;
            }
            return cacheLimitExceeded;
        }
        public static bool ClearTempRateTable(Exception e, DbCommand cmd)
        {
            bool cacheLimitExceeded = false;
            try
            {
                if (e.Message.Contains("The table 'temp_rate' is full"))
                {
                    //rollback is already done, so its safe to call ddl statement without causing implicit commit
                    MediationContext.DropAndCreateTempRateTable(cmd);
                    cacheLimitExceeded = true;
                }
            }
            catch (Exception e1)
            {
                Console.WriteLine(e1);
                throw;
            }
            return cacheLimitExceeded;
        }
    }
}
