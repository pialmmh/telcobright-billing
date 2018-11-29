using System;
using System.Collections.Generic;
using MediationModel;
using FlexValidation;
namespace TelcobrightMediation
{
    public class MefServiceGroupsContainer
    {
        public ServiceGroupComposer CmpServiceGroup = new ServiceGroupComposer();
        public ServiceGroupPreProcessorComposer CmpServiceGroupPre = new ServiceGroupPreProcessorComposer();
        //same dicroute object as in mediation data
        public Dictionary<ValueTuple<int,string>, route> SwitchWiseRoutes = new Dictionary<ValueTuple<int,string>, route>();//<switchid-route,route>
        public IDictionary<string, IServiceGroup> DicExtensions = new Dictionary<string, IServiceGroup>();
        public IDictionary<int, IServiceGroup> IdServiceGroupWiseServiceGroups = new Dictionary<int, IServiceGroup>();
        public Dictionary<int, MefValidator<cdr>> MediationChecklistValidatorForUnAnsweredCdrs { get; set; }
            =new Dictionary<int, MefValidator<cdr>>();//key=sgNumber
        public Dictionary<int, MefValidator<cdr>> MediationChecklistValidatorForAnsweredCdrs { get; set; }
            =new Dictionary<int, MefValidator<cdr>>(); //key=sgNumber

        public Dictionary<int, IServiceGroupPreProcessor> ServiceGroupPreProcessors { get; set; } =
            new Dictionary<int, IServiceGroupPreProcessor>();
    }
}