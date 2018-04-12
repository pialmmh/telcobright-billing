using System.Collections.Generic;
using MediationModel;

namespace TelcobrightMediation.ProductAndContract
{
    public enum UoMRenewalRule
    {
        Reset = 1,
        AddWithPrevious = 2
    }
    public enum BaseProductEventType
    {
        UoMExpired = 1,
    }
    public interface IProductSpecification { }
    public abstract class ProductSpecification<T, TVal>:IProductSpecification
    {
        T Attribute { get; set; }
        TVal Value { get; set; }
    }
    public interface IProduct
    {
        int Id { get; }
        string Name { get; }
        IEnumerable<IProductSpecification> Spec { get; set; }
    }
    
    public interface IProduct<T,TVal>:IProduct where T:class
    {
        T UoM { get; set; }
        TVal UomVal { get; set; }
    }
    
    public abstract class BundledProduct
    {
        List<IProduct> Products { get; set; }
    }
    public abstract class Subscription<T> where T:uom
    {
        public BundledProduct Bundle { get; set; }
        public double Duration { get; set; }//-1=never expires
        public Subscription(BundledProduct Bundle,double Duration)
        {
            this.Bundle = this.Bundle;
            this.Duration = Duration;
        }
    }
    
}
