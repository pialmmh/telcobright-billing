using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;

namespace TelcobrightMediation.Accounting
{
    public class FractionCeilingHelper
    {
        private string Amount { get; }
        private int MaxAllowedFractionPrecissionByTelcobright { get; } = 8;
        private int CeilingAtFractionalPosition { get; }

        public FractionCeilingHelper(decimal amount, int ceilingAtFractionalPosition)
        {
            this.Amount = amount.ToString(CultureInfo.InvariantCulture);
            this.CeilingAtFractionalPosition = ceilingAtFractionalPosition;
            if (this.CeilingAtFractionalPosition >= this.MaxAllowedFractionPrecissionByTelcobright)
            {
                throw new Exception("Ceiling position cannot be >= MaxAllowedFractionPrecissionByTelcobright");
            }
        }

        public decimal GetPreciseDecimal()
        {
            if (this.Amount.Contains(".") == false) return Convert.ToDecimal(this.Amount);
            string[] tempArr = this.Amount.Split('.');
            string nonFracPart = tempArr[0];
            string fracPart = GetFixedLengthFracPart(tempArr[1]);
            string fracpartUpToCeilingPosition = fracPart.Substring(0, this.CeilingAtFractionalPosition+1);

            string fullNumber = new StringBuilder(nonFracPart).Append(".").Append(fracpartUpToCeilingPosition)
                .ToString();
            decimal multiplier = Convert.ToDecimal(Math.Pow(10, this.CeilingAtFractionalPosition));
            return (Math.Ceiling(Convert.ToDecimal(fullNumber) * multiplier) / multiplier);
        }

        private string GetFixedLengthFracPart(string fracPart)
        {

            if (fracPart.Length == this.MaxAllowedFractionPrecissionByTelcobright)
            {
                //fracPart = fracPart;
            }
            else if (fracPart.Length < this.MaxAllowedFractionPrecissionByTelcobright)
            {
                int zeroesToPad = this.MaxAllowedFractionPrecissionByTelcobright - fracPart.Length;
                fracPart = fracPart.PadRight(8, '0');
            }
            else if (fracPart.Length > this.MaxAllowedFractionPrecissionByTelcobright)
            {
                fracPart = fracPart.Substring(0, this.MaxAllowedFractionPrecissionByTelcobright);
            }
            return fracPart;
        }
    }
}
