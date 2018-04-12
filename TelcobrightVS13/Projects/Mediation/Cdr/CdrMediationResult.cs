using System;
using System.Text;
using MediationModel;
using FlexValidation;
namespace TelcobrightMediation
{

    public class CdrMediationResult
    {
        public cdr Cdr { get; }
        public bool MediationComplete => Convert.ToBoolean(this.Cdr.mediationcomplete);
        public int ChargingStatus => Convert.ToInt32(this.Cdr.ChargingStatus);
        public bool Connected => this.Cdr.ConnectTime != null;
        public bool ConnectedByCauseCode => Convert.ToBoolean(this.Cdr.field5);
        public CdrMediationResult(cdr aggregatedCdr)
        {
            this.Cdr = aggregatedCdr;
        }
    }
}