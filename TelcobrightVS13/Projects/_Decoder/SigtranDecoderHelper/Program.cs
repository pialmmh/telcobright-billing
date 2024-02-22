using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAS_DECODER
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                Console.WriteLine($"Decode started for {args[0]}");
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                SigtranPacketDecoder decoder = new SigtranPacketDecoder(args[0]);
                List<SigtranPacket> packets = decoder.GetPackets();
                stopwatch.Stop();
                Console.WriteLine($"Time: {Math.Ceiling((double)stopwatch.ElapsedMilliseconds)}  ms");
                Console.WriteLine($"Task finished for {args[0]}");
            }
            else
            {
                Console.WriteLine("You didn't enter pcap filename");
            }
        }
    }
}
