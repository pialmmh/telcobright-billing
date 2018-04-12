using System.Collections.Generic;
using MediationModel;
using TelcobrightMediation.MefData.GenericAssignment;

namespace TelcobrightMediation
{
    public class MefGenericAssignmentFactoryContainer
    {
        public MefGenericAssignmentFactoryComposer 
            GenericAssignmentFactoryComposer= new MefGenericAssignmentFactoryComposer();
        public IDictionary<string, IGenericParameterAssignmentFactory> DicExtensions = 
            new Dictionary<string,IGenericParameterAssignmentFactory>();
    }
}




