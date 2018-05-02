using MediationModel;

namespace TelcobrightMediation
{
    public interface ISummaryFactory<TSource>
    {
        ISummary CreateNewInstance(TSource summarySourceObject);
    }
}