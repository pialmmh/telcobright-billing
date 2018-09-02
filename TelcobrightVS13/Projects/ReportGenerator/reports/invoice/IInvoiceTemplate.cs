using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportGenerator.reports.invoice
{
    public interface IInvoiceTemplate
    {
        string TemplateName { get; }
        void GenerateInvoice(object data);
        void SaveToPdf(String fileName);
    }
}
