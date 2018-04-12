using System;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using MediationModel;
using TelcobrightMediation.Cdr;

namespace PartnerRules
{

    [Export("NerCalculationRule", typeof(INerCalculationRule))]
    public class NerByCauseCode : INerCalculationRule
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public string HelpText => "NER or CCR calculation rule by cause code";
        public int Id => 1;

        public void ExecuteNerRule(CdrProcessor cdrProcessor, CdrExt cdrExt)
        {
            //mark Call Completion flag (field5) based on cause codes
            int ccField = Convert.ToInt32(cdrProcessor.CdrJobContext.Ne.CcrCauseCodeField);
            if (ccField <= 0)
            {
                return; //-1=CC not be calculated based on release codes, set this in NE.ccrCauseCodeField
            }
            int causeCodeValueInCdr =
                GetCauseCodeValueByFieldNumberAssignedInNeAsCcrCauseCodeField(ccField, cdrExt.Cdr);
            if (cdrProcessor.CdrJobContext.MediationContext.SwitchWiseLookups[cdrProcessor.CdrJobContext.Ne.idSwitch].DicCauseCodeCcr
                    .ContainsKey((cdrProcessor.CdrJobContext.Ne.idSwitch + "-" + causeCodeValueInCdr)) == true)
            {
                cdrExt.Cdr.field5 = 1; //fn.87=ConnectedFlagByCauseCodeForCdrIndicatedInField5
            }
            else
            {
                cdrExt.Cdr.field5 = 0;
            }
        }

        private static int GetCauseCodeValueByFieldNumberAssignedInNeAsCcrCauseCodeField(int ccField, cdr mediatableInstance)
        {
            int causeCodeValue = 0;
            switch (ccField)
            {
                case 23:
                    causeCodeValue = Convert.ToInt32(mediatableInstance.ReleaseCauseSystem);
                    break;
                case 24:
                    causeCodeValue = Convert.ToInt32(mediatableInstance.ReleaseCauseEgress);
                    break;
                case 56:
                    causeCodeValue = Convert.ToInt32(mediatableInstance.releasecauseingress);
                    break;
            }
            if (causeCodeValue == 0) throw new Exception("Cause code value not found in mediated cdr");
            return causeCodeValue;
        }
    }
}
