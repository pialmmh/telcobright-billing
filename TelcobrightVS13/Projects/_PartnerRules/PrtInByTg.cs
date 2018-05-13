﻿using System;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using MediationModel;

namespace PartnerRules
{

    [Export("Partner", typeof(IPartnerRule))]
    public class PrtInByTg : IPartnerRule
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public string RuleName => GetType().Name;
        public string HelpText => "Ingress Partner Identification by Trunk Group";
        public int Id => 1;
        public int Execute(cdr thisCdr, MefPartnerRulesContainer pData)
        {
            var key = new ValueTuple<int,string>(thisCdr.SwitchId, thisCdr.IncomingRoute);
            route thisRoute = null;
            pData.SwitchWiseRoutes.TryGetValue(key, out thisRoute);
            if (thisRoute != null)
            {
                thisCdr.InPartnerId = thisRoute.idPartner;
                return thisRoute.idPartner;
            }
            thisCdr.InPartnerId = 0;
            return 0;
        }
    }
}
