using System.Collections.Generic;
using MediationModel;

namespace LibraryExtensions
{
    public class MefExceptionalCdrPreProcessorContainer
    {
        public ExceptionalCdrPreProcessorComposer composer = new ExceptionalCdrPreProcessorComposer();
        public IDictionary<string, IExceptionalCdrPreProcessor> DicExtensions = new Dictionary<string, IExceptionalCdrPreProcessor>();
    }
}




