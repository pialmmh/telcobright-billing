using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using LibraryExtensions;
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
                Console.WriteLine($"Initial count: {summaries.Count}");
                string json = JsonConvert.SerializeObject(summaries);
                Console.WriteLine($"Size of json data: {json.Length}");
                JsonCompressor<List<sum_voice_day_02>> compressor=new JsonCompressor<List<sum_voice_day_02>>();
                string compressedString = compressor.SerializeToCompressedBase64String(summaries);
                Console.WriteLine($"Size of compressed data {compressedString.Length}");
                summaries = compressor.DeSerializeToObject(compressedString);
                Console.WriteLine($"Count after deserialization: {summaries.Count}");
            }
        }
    }
}
