using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using MediationModel;
using Newtonsoft.Json;
namespace Utils
{
    public class JsonCompressionTester
    {
        public void Test()
        {
            using (PartnerEntities context=new PartnerEntities())
            {
                var summaries = context.sum_voice_day_02.ToList();
                string json = JsonConvert.SerializeObject(summaries);
                Console.WriteLine($"");
            }
        }
    }
}
