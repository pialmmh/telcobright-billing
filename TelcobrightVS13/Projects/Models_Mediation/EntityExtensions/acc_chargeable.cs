using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediationModel
{
    public partial class acc_chargeable
    {
        public ValueTuple<int,int,int> GetTuple()=>new ValueTuple<int, int, int>(this.servicegroup,this.servicefamily,Convert.ToInt32(this.assignedDirection));
    }
}
