using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallConfig
{
    public class UtilMain
    {
        static void Main(string[] args)
        {
            ConfigGeneratorMain generatorMain= new ConfigGeneratorMain();
            generatorMain.Run(new [] {""});
        }

    }
}
