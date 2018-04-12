using System.ComponentModel.Composition;
using System.Collections.Generic;
using Itenso.TimePeriod;
using MediationModel;

namespace TelcobrightMediation.ProductAndContract
{
    [Export("Product", typeof(IProduct))]
    public class DataVolume:IProduct<uom,double>//string=uomId
    {
        public int Id { get { return 101; } }//data proudcts starts from 101
        public string Name { get { return "Data Volume"; } }
        public uom UoM { get; set; }
        public double UomVal { get; set; }
        public IEnumerable<IProductSpecification> Spec { get; set; }//e.g. service level=dedicated/best-effort etc.
        public TimeRange ValidityPeriod { get; set; }
        public DataVolume()
        {
        }
    }
}
