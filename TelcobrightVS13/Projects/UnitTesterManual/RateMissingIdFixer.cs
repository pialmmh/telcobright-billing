using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;
using Newtonsoft.Json;

namespace UnitTesterManual
{
    public class RateMissingIdFixer
    {
        public void Fix(string path, PartnerEntities context)
        {
            List<rate> rates = JsonConvert.DeserializeObject<List<rate>>(File.ReadAllText(path));
            int newId = 0;
            rates.ForEach(r => r.id = ++newId);
            string sql = new StringBuilder(StaticExtInsertColumnHeaders.rate)
                .Append(String.Join(",", rates.Select(c => c.GetExtInsertValues()))).ToString();

            context.Database.ExecuteSqlCommand(sql);
            //context.rates.AddRange(rates);
            //context.SaveChanges();

            //using (StreamWriter file = File.CreateText(@"c:\temp\jslRates.json"))
            //{
            //    JsonSerializer serializer = new JsonSerializer();
            //    serializer.Serialize(file, rates);
            //}
        }
    }
}
