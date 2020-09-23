using System.Collections.Generic;
using MediationModel;

namespace TelcobrightMediation
{
    public class MefExceptionalCdrPreProcessorContainer
    {
        public ExceptionalCdrPreProcessorComposer composer = new ExceptionalCdrPreProcessorComposer();
        public IDictionary<string, IExceptionalCdrPreProcessor> DicExtensions = new Dictionary<string, IExceptionalCdrPreProcessor>();
    }
}




