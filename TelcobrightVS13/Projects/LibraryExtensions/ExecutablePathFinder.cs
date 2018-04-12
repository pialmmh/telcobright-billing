using System.IO;

namespace LibraryExtensions
{
    public static class ExecutablePathFinder
    {
        public static string GetExePath()
        {
            //To get the location the assembly normally resides on disk or the install directory
            string path = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            //once you have the path you get the directory with:
            return (Path.GetDirectoryName(path)).Substring(6);
        }
        public static string GetBinPath()
        {
            string path = GetExePath();
            DirectoryInfo dir=(new DirectoryInfo(path)).Parent;
            return dir.FullName;
        }
    }
}
