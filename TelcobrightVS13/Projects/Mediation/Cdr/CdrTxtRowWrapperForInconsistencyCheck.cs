using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation.Cdr
{
    public class CdrTxtRowWrapperForInconsistencyCheck
    {
        private readonly string[] _row;

        public CdrTxtRowWrapperForInconsistencyCheck(string[] row)
        {
            this._row = row;
        }

        public string this[int index] => this._row[index];
    }
}
