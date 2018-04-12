namespace MediationModel
{
    public partial class route
    {
        public override string ToString()
        {
            return this.SwitchId + "-" + this.RouteName;
        }

    }
}