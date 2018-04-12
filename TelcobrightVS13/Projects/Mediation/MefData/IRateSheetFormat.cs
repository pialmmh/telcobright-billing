using System.Collections.Generic;
using MediationModel;

namespace TelcobrightMediation
{
    public interface IRateSheetFormat
    {
        string RuleName { get; }
        string HelpText { get; }
        int Id { get; }

        string GetRates(string filePath, ref List<ratetask> lstRateTask, rateplan rp, bool endAllPrevPrefix, string[] dateFormats);
    }
}