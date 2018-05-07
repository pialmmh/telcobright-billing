using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace Utils
{
    class RateMissingIdFixer
    {
        public void Fix(string path)
        {
            List<rate> rates= JsonConvert.DeserializeObject<List<rate>>(File.ReadAllText(path));
            int newId = 0;
            rates.ForEach(r=>r.id=++newId);

            using (jslEntities context=new jslEntities())
            {
                context.rates.AddRange(rates);
                context.SaveChanges();
            }

            using (StreamWriter file = File.CreateText(@"c:\temp\jslRates.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, rates);
            }
        }
    }
}
