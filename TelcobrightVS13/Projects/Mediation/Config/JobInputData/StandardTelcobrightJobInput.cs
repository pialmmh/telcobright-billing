using MediationModel;

namespace TelcobrightMediation
{
    public class StandardTelcobrightJobInput : ITelcobrightJobInput
    {
        public TelcobrightConfig Tbc { get; }

        public job TelcobrightJob { get; }
        public PartnerEntities Context { get; }
        public StandardTelcobrightJobInput(TelcobrightConfig tbc, job telcobrightJob)
        {
            this.Tbc = tbc;
            this.TelcobrightJob = telcobrightJob;
            this.Context = null;
        }
    }
}