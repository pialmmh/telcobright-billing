using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MatchMtSri
{
    class Program
    {
        static void Main(string[] args)
        {
            ImsiListener imsiListener = new ImsiListener();

            while (true)
            {
                var startTime = imsiListener.GetBPartySyncTime();
                imsiListener.SetStartTime(startTime);
                if (imsiListener.IsMtFullBatchAvailable() && imsiListener.IsSriFullBatchAvailable())
                {
                    Console.WriteLine($"\n[BPartyListener] New CDR records found to update B Party.");
                    Console.WriteLine($"[BPartyListener] Process started...");
                    Console.WriteLine($"[BPartyListener] Fetching CDR rows...");

                    var aggMt = imsiListener.FetchCdrRows();
                    var redirectingNumbers = aggMt.Select(row => row[Fn.Redirectingnumber]).Distinct().ToList();

                    Console.WriteLine($"[BPartyListener] Fetching SRI rows...");
                    var aggSri = imsiListener.FetchSriRows(redirectingNumbers);

                    Console.WriteLine($"[BPartyListener] Aggregating SRI data with CDR...");
                    aggMt = imsiListener.aggregateMtWithSri(aggSri, aggMt);
                    aggMt = aggMt.Where(row => !string.IsNullOrEmpty(row[Fn.TerminatingCalledNumber])).ToList();

                    Console.WriteLine($"[BPartyListener] Updating B Party in CDR table...");
                    imsiListener.updateTerminatingCalledNumberInCdrTable(aggMt, 30000);

                    Console.WriteLine($"[BPartyListener] Deleting old SRI data as part of cleanup process...");
                    imsiListener.DeleteOldData();
                }
                else
                {
                    Console.WriteLine($"\n[BPartyListener] Waiting for new CDR records to add B Party...");
                    Thread.Sleep(60000);
                }

            }
        }
    }
}
