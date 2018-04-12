using TelcobrightMediation;
using System.ComponentModel.Composition;
using MediationModel;

namespace DisplayClass
{

    [Export("DisplayClass", typeof(IDisplayClass))]//change
    public class DcRoute:IDisplayClass//change
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public string RuleName
        {
            get { return "dcRoute"; }//change
        }
        private string _helpText = "Display Class for Object Type route";//change
        public string HelpText
        {
            get { return this._helpText; }
        }
        public int Id
        {
            get { return 1; }//change
        }

        public DcRoute()
        {
            //do any db initialization if required
        }

        public object GetDisplayClass(object oneEntity)//must be of type dcRoute in this case
        {
            route thisEntity = (route)oneEntity;
            route.DisplayClass dc = new route.DisplayClass(thisEntity);
            return dc;
        }

      
    }
}
