using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace util_DateParser
{
    class Program
    {
        class DatePartColumn
        {
            public string[] Expressions;
            public DatePartColumn(string expression)
            {
                this.Expressions = expression.Split('`');
            }
        }
        class DatePartRow
        {
            public List<DatePartColumn> LstColumns = new List<DatePartColumn>();//will have 4 columns always
            public DatePartRow(List<DatePartColumn> lstcolumns)
            {
                this.LstColumns = lstcolumns;
            }
        }
        static void Main(string[] args)
        {
            string dateSep = "/`-`null`,`.";
            string timeSep = "null";
            List<string[]> txtTableNonAm = new List<string[]>();
            List<string[]> txtTableUs = new List<string[]>();
            List<string[]> txtTableUk = new List<string[]>();
            string[] nonAmbiguous1 = { "yyyy", "MMM`MM`M", "dd`d", "null`HH:mm:ss`hh:mm:ss tt`HH:mm`hh:mm tt`HH.mm.ss`hh.mm.ss tt`HH.mm`hh:mm tt`hh.mm.sstt`hh:mmtt`HHmmss`hhmmss tt`HHmm`hhmm tt`hhmmsstt`hhmmtt" };
            string[] nonAmbiguous2 = { "dd`d", "MMM", "yyyy", "null`HH:mm:ss`hh:mm:ss tt`HH:mm`hh:mm tt`HH.mm.ss`hh.mm.ss tt`HH.mm`hh:mm tt`hh.mm.sstt`hh:mmtt`HHmmss`hhmmss tt`HHmm`hhmm tt`hhmmsstt`hhmmtt" };
            string[] nonAmbiguous3 = { "dd`d", "MMM", "yy", "null`HH:mm:ss`hh:mm:ss tt`HH:mm`hh:mm tt`HH.mm.ss`hh.mm.ss tt`HH.mm`hh:mm tt`hh.mm.sstt`hh:mmtt`HHmmss`hhmmss tt`HHmm`hhmm tt`hhmmsstt`hhmmtt" };
            string[] nonAmbiguous4 = { "MMM", "dd`d", "yyyy", "null`HH:mm:ss`hh:mm:ss tt`HH:mm`hh:mm tt`HH.mm.ss`hh.mm.ss tt`HH.mm`hh:mm tt`hh.mm.sstt`hh:mmtt`HHmmss`hhmmss tt`HHmm`hhmm tt`hhmmsstt`hhmmtt" };
            string[] nonAmbiguous5 = { "MMM", "dd`d", "yy", "null`HH:mm:ss`hh:mm:ss tt`HH:mm`hh:mm tt`HH.mm.ss`hh.mm.ss tt`HH.mm`hh:mm tt`hh.mm.sstt`hh:mmtt`HHmmss`hhmmss tt`HHmm`hhmm tt`hhmmsstt`hhmmtt" };
            txtTableNonAm.Add(nonAmbiguous1);
            txtTableNonAm.Add(nonAmbiguous2);
            txtTableNonAm.Add(nonAmbiguous3);
            txtTableNonAm.Add(nonAmbiguous4);
            txtTableNonAm.Add(nonAmbiguous5);

            string[] us = { "MM`M", "dd`d", "yyyy`yy", "null`HH:mm:ss`hh:mm:ss tt`HH:mm`hh:mm tt`HH.mm.ss`hh.mm.ss tt`HH.mm`hh:mm tt`hh.mm.sstt`hh:mmtt`HHmmss`hhmmss tt`HHmm`hhmm tt`hhmmsstt`hhmmtt" };
            txtTableUs.Add(us);

            string[] uk = { "dd`d", "MM`M", "yyyy`yy", "null`HH:mm:ss`hh:mm:ss tt`HH:mm`hh:mm tt`HH.mm.ss`hh.mm.ss tt`HH.mm`hh:mm tt`hh.mm.sstt`hh:mmtt`HHmmss`hhmmss tt`HHmm`hhmm tt`hhmmsstt`hhmmtt" };
            txtTableUk.Add(uk);

            List<DatePartRow> lstAllRow = GetDatePartsNormalized(txtTableNonAm);
            List<DatePartRow> lstUsRows = GetDatePartsNormalized(txtTableUs);
            List<DatePartRow> lstUkRows = GetDatePartsNormalized(txtTableUk);

            lstUsRows = MylistConcat(lstAllRow, lstUsRows);
            lstUkRows = MylistConcat(lstAllRow, lstUkRows);

            string binFolder = Path.GetDirectoryName(
      System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            binFolder=binFolder.Substring(6, binFolder.Length - 6);

            WriteFile(dateSep, timeSep, lstUsRows,binFolder + "\\usdates.txt");
            WriteFile(dateSep, timeSep, lstUkRows, binFolder + "\\ukdates.txt");

            Console.WriteLine("Date Formats written to files successfully!");
            Console.ReadKey();

        }

        static List<DatePartRow> MylistConcat(List<DatePartRow> lst1, List<DatePartRow> lst2)
        {
            List<DatePartRow> newList = new List<DatePartRow>();
            foreach (DatePartRow thisRow in lst1)
            {
                newList.Add(thisRow);
            }
            foreach (DatePartRow thisRow in lst2)
            {
                newList.Add(thisRow);
            }
            return newList;
        }

        static List<DatePartRow> GetDatePartsNormalized(List<string[]> txtTable)
        {
            List<DatePartRow> lstAllRows = new List<DatePartRow>();
            foreach (string[] row in txtTable)//for rows
            {
                DatePartRow thisRow = null;
                List<DatePartColumn> lstColumns = new List<DatePartColumn>();
                foreach (string col in row)//for columns
                {
                    DatePartColumn thisColumn= new DatePartColumn(col);
                    lstColumns.Add(thisColumn);
                }
                thisRow = new DatePartRow(lstColumns);
                lstAllRows.Add(thisRow);
            }
            return lstAllRows;
        }
        static void WriteFile(string dateSep, string timeSep, List<DatePartRow> lstRow,string fileName)
        {
            List<string> lstParseFormats = new List<string>();
            
            string[] datesepArr = dateSep.Replace("null"," ").Split('`');
            string[] timesepArr = timeSep.Replace("null", " ").Split('`');
            foreach (DatePartRow row in lstRow)
            {
                List<string> col01 = new List<string>();
                List<string> col012 = new List<string>();
                List<string> col0123 = new List<string>();

                List<string>thisList = row.LstColumns[0].Expressions.ToList();
                List<string> nextList = row.LstColumns[1].Expressions.ToList();
                foreach (string separator in datesepArr)
                {
                    col01.AddRange(ConcatTwoLists(thisList, nextList, separator,1));
                }
                

                thisList = col01;
                nextList = row.LstColumns[2].Expressions.ToList();
                foreach (string separator in datesepArr)
                {
                    col012.AddRange(ConcatTwoLists(thisList, nextList, separator,2));
                }
                

                thisList = col012;
                nextList = row.LstColumns[3].Expressions.ToList().Select(s => s.Replace("null", " ")).ToList();
                foreach (string separator in timesepArr)
                {
                    col0123.AddRange(ConcatTwoLists(thisList, nextList, separator,3));
                }


                lstParseFormats.AddRange(col0123);
            }//for each row

            File.WriteAllText(fileName, "'"+string.Join("`", lstParseFormats)+"'");
            
        }

        static List<string> ConcatTwoLists(List<string> list1,List<string> list2,string separator,int level)
        {
            
            List<string> combined = new List<string>();
            foreach (string str1 in list1)
            {
                foreach (string str2 in list2)
                {
                    if (level == 2)
                    {
                        if (str1.Contains(separator) == true)
                        {
                            combined.Add((str1 + separator + str2).Trim());
                        }
                    }
                    else
                    {
                        combined.Add((str1 + separator + str2).Trim());
                    }
                    
                }
            }
            return combined;
        }

    }
}
