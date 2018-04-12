using TelcobrightMediation.Cdr;
using System.Collections.Generic;
using MediationModel;
using TelcobrightMediation.Accounting;
namespace TelcobrightMediation
{
    public interface IServiceFamily
    {
        string RuleName { get; }
        string HelpText { get; }
        int Id { get; }
        AccChargeableExt Execute(CdrExt cdrExt, ServiceContext serviceContext, bool flagLcr);
        //int GetChargedOrChargingPartnerId(CdrExt newCdrExt, ServiceContext serviceContext);//thisrow,assignDir,return idPartner who's charged or charging
        acc_transaction GetTransaction(AccChargeableExt accChargeableExt,ServiceContext serviceContext);
    }
}