using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LibraryExtensions
{
    public static class FileAndPathHelperReadOnly
    {
        public static string GetCurrentExecPath()
        {
            string binFolder = Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            binFolder = binFolder.Substring(6, binFolder.Length - 6);
            return binFolder;
        }
        public static string getBinPath()
        {
            string curDir= Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            curDir = curDir.Substring(6, curDir.Length - 6);
            if (curDir.EndsWith("bin")) return curDir;
            DirectoryInfo di = new DirectoryInfo(curDir).Parent;
            curDir = di.FullName;
            if (curDir.EndsWith("bin"))
            {
                return curDir;
            }
            else throw new Exception("Could not locate bin path.");
        }

        

        //public static bool IsFileSizeConstantOverAPeriod(FileInfo file, int checkIntervalInSeconds, int noOfChecks)
        //{
        //    var initialSize = file.Length;
        //    List<long> sizeCheckResults = new List<long> {initialSize};
        //    for (int i = 0; i < noOfChecks; i++)
        //    {
        //        Thread.Sleep(checkIntervalInSeconds);
        //        FileInfo sameFile = new FileInfo(file.FullName);
        //        sizeCheckResults.Add(sameFile.Length);
        //    }
        //    bool sizeRemainedConstant = true;
        //    foreach (long size in sizeCheckResults)
        //    {
        //        if (size != initialSize)
        //        {
        //            sizeRemainedConstant= false;
        //        }
        //    }
        //    return sizeRemainedConstant;
        //}
    }
}
