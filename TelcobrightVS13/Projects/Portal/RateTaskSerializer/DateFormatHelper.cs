using TelcobrightMediation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using MediationModel;
using Process = System.Diagnostics.Process;

namespace RateTaskSerializer
{
    public class DateFormatHelper
    {
        public static string[] GetDateFormats(string dateParseSelector)
        {
            string parseString = "";
            using (PartnerEntities context = new PartnerEntities())
            {
                parseString = context.enumdateparsestrings.Where(c => c.value == dateParseSelector).First().ParseString;
            }
            string[] dateFormats = parseString.Split('`');
            int count = dateFormats.GetLength(0);
            for (int i = 0; i < count; i++)
            {
                dateFormats[i] = dateFormats[i].Trim();
            }
            return dateFormats;
        }
    }
}
