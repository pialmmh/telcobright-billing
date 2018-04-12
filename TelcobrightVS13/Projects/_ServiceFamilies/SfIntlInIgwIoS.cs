using System;
using System.ComponentModel.Composition;
using System.Globalization;
using LibraryExtensions;
using System.Collections.Generic;
using TelcobrightMediation.Accounting;
using MediationModel;
using TelcobrightMediation.Cdr;

namespace TelcobrightMediation
{

    [Export("ServiceFamily", typeof(IServiceFamily))]
    public class SfIntlInIgwIoS : IServiceFamily
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public string RuleName => GetType().Name;
        public string HelpText => "International Incoming Rev Share [IGW-Old], Before IOF";
        public int Id => 12;
        public AccChargeableExt Execute(CdrExt cdrExt, ServiceContext serviceContext, bool flagLcr)
        {
            throw new NotImplementedException();
        }

        public acc_transaction GetTransaction(AccChargeableExt accChargeableExt, ServiceContext serviceContext)
        {
            throw new NotImplementedException();
        }

        public int GetChargedOrChargingPartnerId(CdrExt newCdrExt,ServiceContext serviceContext)//thisrow,assignDir,return idPartner who's charged or charging
        {
            return Convert.ToInt32(newCdrExt.Cdr.outPartnerId);
        }
        public void Execute(CdrExt cdrExt,ServiceContext serviceContext)
        {
            //write proper acc_billble generator logic if there is a new IOS customer someday.
            //until then, throw
            throw new Exception("International Incoming for IOS is not fully implemented");
            //cdr thisCdr = eventCdr.NewCdrExt.Cdr;
            //common for each service family ********
            //int serviceFamily = Id;
            //PrefixMatcher pr = new PrefixMatcher(serviceFamily, serviceContext.CdrJob.MediationContext.RatingData, serviceContext.AssignDir);
            //DateTime answerTime = new DateTime();
            //if (thisCdr[Fn.Chargingstatus] == "1")
            //{
            //    if (DateTime.TryParseExact(thisCdr[Fn.Answertime], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture,
            //        DateTimeStyles.None, out answerTime) == false)
            //    {
            //        throw new Exception("Could not parse Answertime!");//answer time can't be null for suc.calls
            //    }
            //}
            //else//failed call, use start time as answertime, for prefix matching, duration and amount will be 0
            //{
            //    if (DateTime.TryParseExact(thisCdr[Fn.Starttime], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture,
            //        DateTimeStyles.None, out answerTime) == false)
            //    {
            //        throw new Exception("Could not parse Answertime!");//answer time can't be null for suc.calls
            //    }
            //}
            //int tempCategory = -1;//default 1=call
            //int tempSubCategory = -1;//default 1=voice
            //int.TryParse(thisCdr[Fn.Category], out tempCategory);
            //int.TryParse(thisCdr[Fn.Subcategory], out tempSubCategory);
            //int category = tempCategory > 0 ? tempCategory : 1;//default 1=call
            //int subCategory = tempSubCategory > 0 ? tempSubCategory : 1;//default 1=voice
            //double durationSec = 0;
            ////end common******************************

            //string phoneNumber = thisCdr[Fn.Terminatingcallednumber];//change per service family#####
            //if (phoneNumber.Trim() == "") return;//term no can be empty e.g. for failed calls

            //double.TryParse(thisCdr[Fn.Durationsec], out durationSec);//may change per service family ######

            //List<TupleByPeriod> tups = GetServiceTuple(thisCdr, serviceContext.CdrJob.MediationContext.RatingData, serviceContext.AssignDir, answerTime);
            //if (tups == null) return;
            //Rateext thisRate = pr.PrefixMatching(phoneNumber, category, subCategory, tups, answerTime, flagLcr);
            //if (thisRate == null) return;

            //long finalDuration = 0;//xyz is rounded always

            //finalDuration = Convert.ToInt64(pr.A2ZDuration(Convert.ToDouble(thisCdr[Fn.Durationsec]), thisRate));
            //double terminatingAmount = 0;
            //terminatingAmount = pr.A2ZAmount(finalDuration, thisRate, 0, serviceContext.CdrJob);
            //double btrcAmount = terminatingAmount * (double)thisRate.OtherAmount1 / 100;
            //double icxAmount = terminatingAmount * (double)thisRate.OtherAmount2 / 100;
            //double ansAmount = terminatingAmount * (double)thisRate.OtherAmount3 / 100;

            //thisCdr[Fn.Costvatcommissionin] = btrcAmount.ToString();
            //thisCdr[Fn.Costicxin] = icxAmount.ToString();
            //thisCdr[Fn.Costansin] = ansAmount.ToString();
            //thisCdr[Fn.Matchedprefixsupplier] = thisRate.Prefix;
            //thisCdr[Fn.Roundedduration] = finalDuration.ToString();

            //if (thisRate != null)
            //{
            //    return;
            //    //write appropriate billable params here for IoS if get a customer someday
            //    //billable should include ICX & ANS charge
            //    //return new acc_transaction
            //    //{

            //    //};
            //}
            //else
            //{
            //    return;
            //}
        }
        

        
    }
}
