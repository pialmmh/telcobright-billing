namespace MediationModel
{
    public partial class job
    {
        public object AdditionalData { get; set; }
        public override string ToString()
        {
            return this.ne.SwitchName + this.JobName;
        }
    }
}