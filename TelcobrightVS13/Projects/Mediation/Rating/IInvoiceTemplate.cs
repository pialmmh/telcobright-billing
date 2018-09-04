using System;

namespace TelcobrightMediation
{
    public interface IInvoiceTemplate
    {
        string TemplateName { get; }
        void GenerateInvoice(object data);
        void SaveToPdf(String fileName);
    }
}
