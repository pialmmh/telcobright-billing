using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;

namespace TelcobrightMediation.RatingAndInvoicing
{
    public interface IInvoiceTemplate
    {
        string TemplateName { get; }
        void GenerateInvoice(object data);
    }

    [Export("InvoiceTemplate", typeof(IInvoiceTemplate))]
    public class IcxIntlOutInvoiceTemplate : IInvoiceTemplate
    {
        public string TemplateName => this.GetType().Name;
        public void GenerateInvoice(object data)
        {
            //serialize or casting logic, can be different for each template
            List<sum_voice_day_02> summaries = (List<sum_voice_day_02>) data;
            //set as data source
            //save as
            //show
        }

        private void GetDesignTemplate()
        {
            
        }
    }
}
