namespace TelcobrightMediation.Accounting
{
    public interface IBillingRule
    {
        string RuleName { get; }
        string Description { get; }
        bool IsPrepaid { get; set; }
    }
}