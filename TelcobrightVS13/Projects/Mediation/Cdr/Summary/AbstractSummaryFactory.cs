using MediationModel;
using System.Collections.Generic;

namespace TelcobrightMediation
{
    public abstract class AbstractSummaryFactory<TSource> : ISummaryFactory<TSource>
    {
        public abstract ISummary CreateNewInstance(TSource summarySourceObject);
    }
}
