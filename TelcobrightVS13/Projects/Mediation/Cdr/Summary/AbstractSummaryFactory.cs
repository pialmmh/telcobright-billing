using MediationModel;
using System.Collections.Generic;

namespace TelcobrightMediation
{
    public interface ISummaryFactory<TSource>
    {
        ISummary CreateNewInstance(TSource summarySourceObject);
    }
    public abstract class AbstractSummaryFactory<TSource> : ISummaryFactory<TSource>
    {
        public abstract ISummary CreateNewInstance(TSource summarySourceObject);
    }
}
