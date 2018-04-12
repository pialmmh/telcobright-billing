using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;

namespace TelcobrightMediation.MefData.GenericAssignment
{
    public interface IGenericParameterAssignmentFactory
    {
        string FactoryName { get; }
        void GetParametersAsTypedInstance(Dictionary<string,string> parameters,
            Action<Dictionary<string, string>> methodToCreateParameterObjectContainer);
        void WriteAssignmentToDb(genericparameterassignment genericparameterassignment,PartnerEntities context);
    }
}
