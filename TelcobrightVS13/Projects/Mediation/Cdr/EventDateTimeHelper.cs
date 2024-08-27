using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;
using TelcobrightMediation;
using TelcobrightMediation.Config;

namespace TelcobrightMediation
{
    public class EventDateTimeHelper
    {
        public static int getTimeFieldNo(CdrSetting cdrSettings, string[] row)
        {
            int timeFieldNo = -1;
            switch (cdrSettings.SummaryTimeField)
            {
                case SummaryTimeFieldEnum.StartTime:
                    timeFieldNo = Fn.StartTime;
                    break;
                case SummaryTimeFieldEnum.AnswerTime:
                    timeFieldNo = Fn.AnswerTime;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return timeFieldNo;
        }
        public static string getTimeFieldName(CdrSetting cdrSettings, string[] row)
        {
            switch (cdrSettings.SummaryTimeField)
            {
                case SummaryTimeFieldEnum.StartTime:
                    return "starttime";
                case SummaryTimeFieldEnum.AnswerTime:
                    return "answertime";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
