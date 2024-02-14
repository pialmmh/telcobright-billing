using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
//using IgwModel;
using System.Text;
using LibraryExtensions;
using MediationModel;
/// <summary>
/// Summary description for myExcel
/// </summary>
public class csvArrayHelper
{
    public static string GetArrayFromCsv(ref object[,] objArray, ref List<string[]> strLines, ref char sepCharForCsv)
    {
        try
        {
            char[] sepArr = new char[] { ',', ':', ';', '`' };
            sepCharForCsv = ',';
            //find separator chracter
            Dictionary<char, int> dicCharCount = new Dictionary<char, int>();
            dicCharCount.Add(',', 0);
            dicCharCount.Add(':', 0);
            dicCharCount.Add(';', 0);
            dicCharCount.Add('`', 0);

            int dim1 = objArray.GetLength(0);
            int dim2 = objArray.GetLength(1);
            //count occurance of each char, max will be the separator
            for (int i = 1; i <= dim1; i++)
            for (int j = 1; j <= dim2; j++)
                foreach (char ch in sepArr)
                {
                    dicCharCount[ch] += objArray[i, j].ToString().Count(c => c == ch);
                }
            sepCharForCsv = (from x in dicCharCount where x.Value == dicCharCount.Max(v => v.Value) select x.Key)
                .First();
            //split csv rows to multiple field row
            for (int i = 1; i <= dim1; i++)
            {
                strLines.Add(objArray[i, 1].ToString().Split(sepCharForCsv));
            }

            return "";
        }
        catch (Exception e1)
        {
            Console.WriteLine(e1);
            return e1.Message;
        }
    }

}