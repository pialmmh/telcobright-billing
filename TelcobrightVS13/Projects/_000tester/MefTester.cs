using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
namespace Utils
{
    public class MefPort
    {
        [ImportMany(typeof(int[]))]
        private IEnumerable<int[]> LstIntArray;
        public Dictionary<string,int[]> DicIntArray { get; set; }
    }
}
