using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using MediationModel;
using Newtonsoft.Json;

namespace TelcobrightMediation
{
    public class InvoiceSectionDataRetriever<T>
    {
        public List<T> GetSectionData(invoice invoice,int sectionNumber)
        {
            invoice_item invoiceItem = invoice.invoice_item.Single();
            Dictionary<string, string> invoiceMap =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(invoiceItem.JSON_DETAIL);
            List<T> sectionData = invoiceMap.Where(kv => 
                kv.Key.StartsWith("Section-" + sectionNumber.ToString()))
                .Select(kv => new JsonCompressor<List<T>>().DeSerializeToObject(kv.Value)).Single();
            return sectionData;
        }
    }
}
