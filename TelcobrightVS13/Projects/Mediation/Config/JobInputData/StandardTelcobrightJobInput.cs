using MediationModel;

namespace TelcobrightMediation
{
    public class StandardTelcobrightJobInput : ITelcobrightJobInput
    {
        public TelcobrightConfig Tbc { get; }

        public job Job { get; }
        public PartnerEntities Context { get; }
        public StandardTelcobrightJobInput(TelcobrightConfig tbc, job telcobrightJob)
        {
            this.Tbc = tbc;
            this.Job = telcobrightJob;
            this.Context = null;
        }
    }
}