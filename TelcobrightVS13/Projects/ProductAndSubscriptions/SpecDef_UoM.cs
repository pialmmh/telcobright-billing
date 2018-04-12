using MediationModel;
using System.ComponentModel.Composition;
using TelcobrightMediation;
using TelcobrightMediation.ProductAndContract;
using System;
namespace TelcobrightMediation.ProductAndContract
{
    public class UoM_Double : IProductSpecification<uom, double>
    {
        public uom attribute { get; set; }
        public double value { get; set; }
    }
    public class UoM_Int : IProductSpecification<uom, int>
    {
        public uom attribute { get; set; }
        public int value { get; set; }
    }
    public class UoM_Long : IProductSpecification<uom, long>
    {
        public uom attribute { get; set; }
        public long value { get; set; }
    }
}
