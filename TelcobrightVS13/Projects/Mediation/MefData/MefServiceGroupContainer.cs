using System.Collections.Generic;
using MediationModel;
using FlexValidation;
namespace TelcobrightMediation
{
    public class MefServiceGroupsContainer
    {
        public ServiceGroupComposer CmpServiceGroup = new ServiceGroupComposer();
        //same dicroute object as in mediation data
        public Dictionary<string, route> DicRouteIncludePartner = new Dictionary<string, route>();//<switchid-route,route>
        public IDictionary<string, IServiceGroup> DicExtensions = new Dictionary<string, IServiceGroup>();
        public IDictionary<int, IServiceGroup> IdServiceGroupWiseServiceGroups = new Dictionary<int, IServiceGroup>();
        public Dictionary<int, FlexValidator<cdr>> MediationChecklistValidatorForUnAnsweredCdrs =
            new Dictionary<int, FlexValidator<cdr>>();//key=sgNumber
        public Dictionary<int, FlexValidator<cdr>> MediationChecklistValidatorForAnsweredCdrs =
            new Dictionary<int, FlexValidator<cdr>>(); //key=sgNumber
    }
}