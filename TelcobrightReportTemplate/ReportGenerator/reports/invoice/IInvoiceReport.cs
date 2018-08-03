using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportGenerator.reports.invoice
{
    public interface IInvoiceReport
    {
        InvoiceReportType reportType { get; }

        void saveToPdf(String fileName);
    }
}
