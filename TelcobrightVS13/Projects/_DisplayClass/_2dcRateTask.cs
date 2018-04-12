using TelcobrightMediation;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MediationModel;

namespace DisplayClass
{

    [Export("DisplayClass", typeof(IDisplayClass))]//change
    public class DcRateTask:IDisplayClass//change
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public string RuleName
        {
            get { return "dcRateTask"; }//change
        }
        private string _helpText = "Display Class for Object Type Rate Task during export";//change
        public string HelpText
        {
            get { return this._helpText; }
        }
        public int Id
        {
            get { return 2; }//change
        }
        private Dictionary<string, countrycode> _dicCountry = null;
        public DcRateTask()//constructor will load any data from db if required
        {
            using (PartnerEntities context = new PartnerEntities())
            {
                this._dicCountry = context.countrycodes.ToDictionary(c => c.Code);
            }
        }

        public object GetDisplayClass(object oneEntity)
        {
            
            ratetask thisEntity = (ratetask)oneEntity;//change
            ratetask.DisplayClass dc = new ratetask.DisplayClass(thisEntity, this._dicCountry);//change
            return dc;
        }




    }
}
