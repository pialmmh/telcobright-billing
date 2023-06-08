using System;
using System.IO;
namespace InstallConfig
{
    public class FileRename
    {
        public void AppendPrefix(string srcDir,string prefix)
        {
            var dir = new DirectoryInfo(srcDir);
            int count = 0;
            foreach (FileInfo file in dir.GetFiles())
            {
                Console.WriteLine("Progress: "+ ++count +", Appending prefix to:" + file.Name);
                File.Move(file.FullName, dir.FullName + Path.DirectorySeparatorChar +
                    prefix + file.Name);
            }
        }
    }
}
