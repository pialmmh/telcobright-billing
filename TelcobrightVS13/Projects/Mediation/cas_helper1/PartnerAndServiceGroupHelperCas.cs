using System.ComponentModel.Composition;
using System;
using MediationModel;
using System.Collections.Generic;
using System.Linq;
using LibraryExtensions;
using MediationModel.enums;
using Newtonsoft.Json;
using TelcobrightMediation.Accounting;
using TelcobrightMediation.Accounting.Invoice;
using TelcobrightMediation.Cdr;
using TransactionTuple = System.ValueTuple<int, int, long, int, long>;
namespace TelcobrightMediation
{

    public class PartnerAndServiceGroupHelperCas
    {
        Dictionary<int, partner> Partners { get; set; }
        public PartnerAndServiceGroupHelperCas(Dictionary<int,partner> partners)
        {
            this.Partners = partners;
        }
        public partner getInPartner(cdr thisCdr)
        {
            int inPartner = Convert.ToInt32(thisCdr.InPartnerId);
            if (inPartner > 0)
            {
                partner pIn = null;
                if (Partners.TryGetValue(inPartner, out pIn) == true)
                {
                    int outPartner = 0;
                    partner pOut = null;
                    if (Partners.TryGetValue(outPartner, out pOut) == true)
                    {
                        return pIn;
                    }
                }
            }
            return null;
        }
        public partner getOutPartner(cdr thisCdr)
        {
            int inPartner = Convert.ToInt32(thisCdr.InPartnerId);
            if (inPartner > 0)
            {
                partner pIn = null;
                if (Partners.TryGetValue(inPartner, out pIn) == true)
                {
                    int outPartner = 0;
                    partner pOut = null;
                    if (Partners.TryGetValue(outPartner, out pOut) == true)
                    {
                        return pOut;
                    }
                }
            }
            return null;
        }
        public int checkAndSetLtfs(cdr thisCdr)
        {
            partner pIn = getInPartner(thisCdr);
            if (pIn != null)
            {
                partner pOut = getOutPartner(thisCdr);
                if (pOut != null)
                {
                    if (pIn.PartnerType == IcxPartnerType.ANS &&
                        pOut.PartnerType == IcxPartnerType.ANS) //ANS and route=national
                    {
                        if(thisCdr.OriginatingCalledNumber.StartsWith("0800")) return 1;
                    }
                }
            }
            return 0;
        }
        public int checkAndSetDomestic(cdr thisCdr)
        {
            partner pIn = getInPartner(thisCdr);
            if (pIn != null)
            {
                partner pOut = getOutPartner(thisCdr);
                if (pOut != null)
                {
                    if (pIn.PartnerType == IcxPartnerType.ANS &&
                        pOut.PartnerType == IcxPartnerType.ANS) //ANS and route=national
                    {
                        return 1;
                    }
                }
            }
            return 0;
        }
        public int checkAndSetIntlIn(cdr thisCdr)
        {
            partner pIn = getInPartner(thisCdr);
            if (pIn != null)
            {
                partner pOut = getOutPartner(thisCdr);
                if (pOut != null)
                {
                    if (pIn.PartnerType == IcxPartnerType.IOS &&
                        pOut.PartnerType == IcxPartnerType.ANS) //ANS and route=national
                    {
                        return 3;
                    }
                }
            }
            return 0;
        }
        public int checkAndSetIntlOut(cdr thisCdr)
        {
            partner pIn = getInPartner(thisCdr);
            if (pIn != null)
            {
                partner pOut = getOutPartner(thisCdr);
                if (pOut != null)
                {
                    if (pIn.PartnerType == IcxPartnerType.ANS &&
                        pOut.PartnerType == IcxPartnerType.IOS) //ANS and route=national
                    {
                        return 2;
                    }
                }
            }
            return 0;
        }
    }
}
