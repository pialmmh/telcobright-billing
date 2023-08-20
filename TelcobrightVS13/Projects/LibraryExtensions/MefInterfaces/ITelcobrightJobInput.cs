using MediationModel;

namespace LibraryExtensions
{
    public interface ITelcobrightJobInput
    {
        job TelcobrightJob { get; }
        PartnerEntities Context { get; }
    }
}