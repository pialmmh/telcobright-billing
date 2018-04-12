using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;
using OfbizModel;
using TelcobrightHelper;
using Itenso.TimePeriod;
namespace TelcobrightMediation.ProductAndContract
{
    public abstract class ContractExecutedEventArgs
    {

    }
    public abstract class ContractExpiredEventArgs
    {

    }
    public interface IContract
    {
        string name { get; }
        DateTime startDate { get; set; }
        event EventHandler<ContractExecutedEventArgs> OnExecuted;
    }
    
    public interface IPeriodCommittable<T> where T:uom
    {
        uom commitmentPeriod { get; set; }
        double value { get; set; }
    }
    public interface IExpirable
    {
        TimeRange validityPeriod { get; set; }
        event EventHandler<ContractExpiredEventArgs> OnExpired;
    }
    public interface IRenewable
    {
        DateTime newStartDate { get; set; }
    }
    public interface IPurchasable//e.g. 40G data, 2Mbps speed
    {
        int pricingRule { get; set; }
    }
    public interface IPrepaid
    {

    }
    public interface IPostpaid
    {

    }
    public interface IBillable
    {
        string GetbillingAccountPrefix(int idServiceGroup);
    }
    public interface IServiceContract<T> : IContract where T : class//contract of bundled or base product
    {
        T contractedItem { get; set; }
        ServiceAssignmentDirection assignDirection { get; }
    }
}
