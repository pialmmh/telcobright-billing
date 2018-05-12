using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Testers;
namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            StringVsTupPerfTester tester=new StringVsTupPerfTester();
            tester.Test();
        }
    }
}
