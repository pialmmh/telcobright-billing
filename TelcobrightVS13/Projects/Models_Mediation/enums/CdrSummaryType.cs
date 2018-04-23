using System;
using System.Collections.Generic;

namespace MediationModel
{
    public static class CdrSummaryType
    {
        public static string sum_voice_day_01 => "sum_voice_day_01";
        public static string sum_voice_day_02 => "sum_voice_day_02";
        public static string sum_voice_day_03 => "sum_voice_day_03";
        public static string sum_voice_hr_01 => "sum_voice_hr_01";
        public static string sum_voice_hr_02 => "sum_voice_hr_02";
        public static string sum_voice_hr_03 => "sum_voice_hr_03";

        public static Dictionary<string, Type> Types => new Dictionary<string, Type>()
        {
            {"sum_voice_day_01", typeof(sum_voice_day_01)},
            {"sum_voice_day_02", typeof(sum_voice_day_02)},
            {"sum_voice_day_03", typeof(sum_voice_day_03)},
            {"sum_voice_hr_01", typeof(sum_voice_hr_01)},
            {"sum_voice_hr_02", typeof(sum_voice_hr_02)},
            {"sum_voice_hr_03", typeof(sum_voice_hr_03)},
        };
    }
}
