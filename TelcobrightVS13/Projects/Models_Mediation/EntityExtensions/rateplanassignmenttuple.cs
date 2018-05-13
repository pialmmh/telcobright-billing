using System.Text;

namespace MediationModel
{
    public partial class rateplanassignmenttuple
    {
        public override string ToString()
        {
            return this.idService.ToString() + "/" +
                   this.AssignDirection == null ? "null" : this.AssignDirection.ToString() + "/" +
                                                           this.idpartner == null ? "null" : this.idpartner.ToString() + "/" +
                                                                                             this.route == null ? "null" : this.route.ToString() + "/" +
                                                                                                                           this.priority == null ? "null" : this.priority.ToString();
        }

        private string GetServiceTupleWithoutPriority()
        {
            return this.idService.ToString();
        }

        private string GetPartnerTupleWithoutPriority()
        {
            return new StringBuilder(GetServiceTupleWithoutPriority()).Append("/")
                .Append(this.AssignDirection.ToString()).Append("/").Append(this.idpartner.ToString()).ToString();
        }

        private string GetRouteTupleWithoutPriority()
        {
            return new StringBuilder(GetServiceTupleWithoutPriority()).Append("/")
                .Append(this.AssignDirection.ToString()).Append("/").Append(this.route.ToString()).ToString();
        }

        public string GetTuple()
        {
            if (this.route != null && this.route > 0)
                return GetRouteTupleWithoutPriority();
            else if (this.idpartner != null && this.idpartner > 0)
                return GetPartnerTupleWithoutPriority();
            return GetServiceTupleWithoutPriority();
        }
    }
}