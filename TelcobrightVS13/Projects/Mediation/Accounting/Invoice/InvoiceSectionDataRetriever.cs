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
            List<T> sectionData = new List<T>();
            invoice_item invoice_item = invoice.invoice_item.Single();
            Dictionary<string, string> invoiceMap =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(invoice_item.JSON_DETAIL);
            InvoiceSection section = invoiceMap.Where(kv => kv.Key == "Section-" + sectionNumber.ToString())
                .Select(kv => JsonConvert.DeserializeObject<InvoiceSection>(kv.Value)).Single();
            JsonCompressor<List<T>> jsonCompressor = new JsonCompressor<List<T>>();
            sectionData = jsonCompressor.DeSerializeToObject(section.SerializedData);
            return sectionData;
        }
    }
}
