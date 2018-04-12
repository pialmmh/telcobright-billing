using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Testers
{
    public interface A
    {
        int I { get; set; }
    }

    public class B : A
    {
        public int I { get; set; }
    }
    public class C : A
    {
        public int I { get; set; }
    }
    public static class CastWithInterfaceTester
    {
        public static void Test()
        {
            B b=new B();
            C c = (C)(A)b;
        } 
    }
}
