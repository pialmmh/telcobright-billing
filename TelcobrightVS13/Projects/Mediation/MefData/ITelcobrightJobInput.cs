using MediationModel;

namespace TelcobrightMediation
{
    public interface ITelcobrightJobInput
    {
        job Job { get; }
        PartnerEntities Context { get; }
    }
}