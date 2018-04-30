using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using MediationModel;
using MySql.Data.MySqlClient;
using TelcobrightMediation;
using TelcobrightMediation.Cdr;
using CdrSummaryTuple = System.ValueTuple<int, int, int, string, string, decimal, decimal, System.ValueTuple<string, string, string, string, string, string, string, System.ValueTuple<string, string, string, string, string, string>>>;

namespace UnitTesterManual
{
    class DoubleComparer
    {
        private double _tollerance = .00000001;

        public bool IsEqal(double num1, double num2)
        {
            if (Math.Abs(num1 - num2) < this._tollerance)
                return true;
            else return false;
        }

        public DoubleComparer()
        {
        }

        public DoubleComparer(double tollerance)
        {
            this._tollerance = tollerance;
        }
    }
}
