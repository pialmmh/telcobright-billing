using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportGenerator.Helper
{
    public static class CurrencyHelper
    {
        public static string NumberToWords(double doubleNumber)
        {
            // round to 2 decimal
            doubleNumber = Math.Round(doubleNumber, 2);

            var beforeFloatingPoint = (int)Math.Floor(doubleNumber);
            var beforeFloatingPointWord = $"{NumberToWords(beforeFloatingPoint)} dollars";
            var afterFloatingPointWord =
                $"{SmallNumberToWord((int)((doubleNumber - beforeFloatingPoint) * 100), "")} cents";
            return $"{beforeFloatingPointWord} and {afterFloatingPointWord}";
        }

        private static string NumberToWords(int number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));

            var words = "";

            if (number / 1000000000 > 0)
            {
                words += NumberToWords(number / 1000000000) + " billion ";
                number %= 1000000000;
            }

            if (number / 1000000 > 0)
            {
                words += NumberToWords(number / 1000000) + " million ";
                number %= 1000000;
            }

            if (number / 1000 > 0)
            {
                words += NumberToWords(number / 1000) + " thousand ";
                number %= 1000;
            }

            if (number / 100 > 0)
            {
                words += NumberToWords(number / 100) + " hundred ";
                number %= 100;
            }

            words = SmallNumberToWord(number, words);

            return words;
        }

        private static string SmallNumberToWord(int number, string words)
        {
            if (number <= 0) return words;
            if (words != "")
                words += " ";

            var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
            var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

            if (number < 20)
                words += unitsMap[number];
            else
            {
                words += tensMap[number / 10];
                if ((number % 10) > 0)
                    words += "-" + unitsMap[number % 10];
            }
            return words;
        }

        public static string NumberToTakaWords(double Number)
        {
            string[] sArrayPows = new string[3];
            string sSign = string.Empty;
            string sVal;
            string sNumStr;
            string sTaka;
            string sPaisa;
            string sPow;
            int nCommaCount = 0;
            int nDigitCount;

            if (Number < 0) {
                sSign = "Minus";
                Number = Number * -1;
            }

            sArrayPows[1] = "Thousand";
            sArrayPows[2] = "Lac";
            sArrayPows[0] = "Crore";

            sNumStr = String.Format("{0:0.00}", Number);

            sPaisa = HundredWord(Convert.ToInt32(sNumStr.Substring(sNumStr.Length - 2, 2)));

            if (sPaisa.Length != 0)
            {
                sPaisa = "Paisa " + sPaisa;
            }

            sNumStr = sNumStr.Substring(0, sNumStr.Length - 3);

            if (sNumStr.Length >= 3) {
                sTaka = HundredWord(Convert.ToInt32(sNumStr.Substring(sNumStr.Length - 3, 3)));
            } else {
                sTaka = HundredWord(Convert.ToInt32(sNumStr));
            }

            if (sNumStr.Length <= 3) {
                sNumStr = "";
            } else {
                sNumStr = sNumStr.Substring(0, sNumStr.Length - 3);
                nCommaCount = 1;
            }

            while (sNumStr.Length != 0)
            {

                if (nCommaCount % 3 == 0)
                {
                    nDigitCount = 3;
                }
                else
                {
                    nDigitCount = 2;
                }

                if (sNumStr.Length > nDigitCount) {
                    sPow = HundredWord(Convert.ToInt32(sNumStr.Substring(sNumStr.Length - nDigitCount, nDigitCount)));

                    if (sPow.Length > 0) {
                        sPow = sPow + " " + sArrayPows[nCommaCount % 3];
                    }
                    if (sTaka.Length > 0 && sPow.Length > 0) {
                        sPow = sPow + " ";
                    }
                    sNumStr = sNumStr.Substring(0, sNumStr.Length - nDigitCount);
                } else {
                    sPow = HundredWord(Convert.ToInt32(sNumStr)) + " " + sArrayPows[nCommaCount % 3];
                    sNumStr = "";
                }
                sTaka = sPow + " " + sTaka;
                nCommaCount++;
            }

            if (sTaka.Length > 0) {
                sTaka = "Taka " + sTaka;
            }
            sVal = sTaka;
            if (sVal.Length > 0 && sPaisa.Length > 0) {
                sVal = sVal + " And ";
            }

            sVal = sVal + sPaisa;

            if (sVal.Length == 0)
            {
                sVal = "Taka Zero";
            }

            sVal = sSign + sVal + " Only";

            return sVal;
        }

        private static string HundredWord(long nNumber)
        {
            string sVal;
            string[] sArrayDigits = new string[10];
            string[] sArrayTeens = new string[10];
            string[] sArrayTens = new string[10];
            string sNumStr;
            string sPos1;
            string sPos2;
            string sPos3;

            sArrayDigits[0] = "";
            sArrayDigits[1] = "One";
            sArrayDigits[2] = "Two";
            sArrayDigits[3] = "Three";
            sArrayDigits[4] = "Four";
            sArrayDigits[5] = "Five";
            sArrayDigits[6] = "Six";
            sArrayDigits[7] = "Seven";
            sArrayDigits[8] = "Eight";
            sArrayDigits[9] = "Nine";

            sArrayTeens[0] = "Ten";
            sArrayTeens[1] = "Eleven";
            sArrayTeens[2] = "Twelve";
            sArrayTeens[3] = "Thirteen";
            sArrayTeens[4] = "Forteen";
            sArrayTeens[5] = "Fifteen";
            sArrayTeens[6] = "Sixteen";
            sArrayTeens[7] = "Seventeen";
            sArrayTeens[8] = "Eighteen";
            sArrayTeens[9] = "Ninteen";

            sArrayTens[0] = "";
            sArrayTens[1] = "Ten";
            sArrayTens[2] = "Twenty";
            sArrayTens[3] = "Thirty";
            sArrayTens[4] = "Forty";
            sArrayTens[5] = "Fifty";
            sArrayTens[6] = "Sixty";
            sArrayTens[7] = "Seventy";
            sArrayTens[8] = "Eighty";
            sArrayTens[9] = "Ninty";

            sNumStr = nNumber.ToString().PadLeft(3, '0').Substring(0, 3);

            if (sNumStr.Substring(0, 1) != "0")
            {
                sPos1 = sArrayDigits[Convert.ToInt16(sNumStr.Substring(0, 1))] + " Hundred";
            }
            else
            {
                sPos1 = "";
            }

            sNumStr = sNumStr.Substring(sNumStr.Length - 2, 2);

            if (sNumStr.Substring(0, 1) == "1")
            {
                sPos2 = sArrayTeens[Convert.ToInt16(sNumStr.Substring(sNumStr.Length - 1, 1))];
                sPos3 = "";
            }
            else
            {
                sPos2 = sArrayTens[Convert.ToInt16(sNumStr.Substring(0, 1))];
                sPos3 = sArrayDigits[Convert.ToInt16(sNumStr.Substring(sNumStr.Length - 1, 1))];
            }

            sVal = sPos1;
            if (sVal.Length != 0 && sPos2.Length != 0) {
                sVal = sVal + " ";
            }

            sVal = sVal + sPos2;

            if (sVal.Length != 0 && sPos3.Length != 0) {
                sVal = sVal + " ";
            }
            sVal = sVal + sPos3;

            return sVal;
        }
    }
}
