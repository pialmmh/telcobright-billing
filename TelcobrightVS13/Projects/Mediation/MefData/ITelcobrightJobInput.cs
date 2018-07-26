using MediationModel;

namespace TelcobrightMediation
{
    public interface ITelcobrightJobInput
    {
        job TelcobrightJob { get; }
        PartnerEntities Context { get; }
    }
}