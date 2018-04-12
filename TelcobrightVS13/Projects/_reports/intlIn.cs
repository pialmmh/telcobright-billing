using TelcobrightMediation;
using System.ComponentModel.Composition;

namespace reports
{

    [Export("Report", typeof(IReport))]
    public class RptInternationalIn : IReport
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        
        private string _helpText = "Traffic Report Intl in.";
        public string RuleName
        {
            get { return "rtpIntlIncoming"; }
        }

        public string HelpText
        {
            get { return this._helpText; }
        }
        public int Id
        {
            get { return 1; }
        }
        
    }
}
