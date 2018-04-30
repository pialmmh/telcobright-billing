using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using MediationModel;

namespace TelcobrightMediation
{

    public class TelcoBrightAdhocMethods
    {
        void ManualRating()
        {
            List<string[]> lines = ParseTextFileToList(@"c:\outgoing23_31.txt", '`', 0);
            List<string> lstSql = new List<string>();
            Dictionary<string, rate> dicRates = null;
            using (PartnerEntities conPartner = new PartnerEntities(ConfigurationManager.ConnectionStrings["dbl"].ConnectionString))
            {
                dicRates = conPartner.rates.Where(c => c.idrateplan == 29).ToDictionary(c => c.Prefix);
            }

            using (MySqlConnection con = new MySqlConnection("server=127.0.0.1;User Id=root;password=Takay1#$ane;Persist Security Info=True;database=roots;"))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand("", con))
                {
                    int count = 0;
                    List<string> lstSegment = new List<string>();
                    foreach (string[] thisrow in lines)
                    {
                        string prefix = thisrow[Fn.MatchedPrefixY];
                        double rDuration = Convert.ToDouble(thisrow[Fn.RoundedDuration]);
                        double rateAmount = Convert.ToDouble(dicRates[prefix].OtherAmount1);
                        double amount = rDuration * rateAmount / 60;
                        thisrow[Fn.YAmount] = amount.ToString();
                        int segment = 5000;

                        lstSegment.Add((" ('" + string.Join("','", thisrow) + "') ").Replace("'\\N'", "NULL"));
                        if ((count > 0) && (count % segment == 0))
                        {
                            cmd.CommandText = " insert into cdrtemploaded values " +
                            string.Join(",", lstSegment) + ";";
                            cmd.ExecuteNonQuery();
                            lstSegment = new List<string>();
                            GC.Collect();
                            Console.WriteLine("progress:" + count);
                        }
                        count++;
                    }
                    cmd.CommandText = " insert into cdrtemploaded values " +
                            string.Join(",", lstSegment) + ";";
                    cmd.ExecuteNonQuery();
                    lstSegment = new List<string>();
                    GC.Collect();
                    Console.WriteLine("progress:" + count);

                }
            }
        }

        List<string[]> ParseTextFileToList(string path, char seperator, int linesToSkipBefore)
        {
            List<string[]> parsedData = new List<string[]>();

            using (StreamReader readFile = new StreamReader(path))
            {
                string line;
                string[] row;
                int thisLine = 1;
                while ((line = readFile.ReadLine()) != null)
                {
                    if (thisLine <= linesToSkipBefore)
                    {
                        thisLine += 1;
                        continue;
                    }
                    row = line.Split(seperator);
                    parsedData.Add(row);
                    thisLine += 1;
                }
            }

            return parsedData;
        }
    }

}
