using System;
using System.Collections.Generic;
namespace TelcobrightFileOperations
{
    public class FileLocation
    {
        public string Name { get; set; }
        public string LocationType { get; set; }
        public string OsType { get; set; }
        public string PathSeparator { get; set; }//char caused problem with json serializing
        public string ServerIp { get; set; }
        public string StartingPath { get; set; }
        public string User { get; set; }
        public string Pass { get; set; }
        public string Sftphostkey { get; set; }
        public DateTime ExcludeBefore { get; set; }//ad-hoc quick param for DBL to exclude files before June 26 2015
        public int IgnoreZeroLenghFile { get; set; }
        public bool Skip { get; set; }
        public Dictionary<string,string> ExistingPaths { get; set; }
        public string RelativeCurrentDirectory { get; set; }

        public FileLocation()
        {
            this.ExistingPaths = new Dictionary<string, string>();
            
        }
        public string GetFullUrlWithServer()
        {
            if (this.StartingPath.Trim() != "")
            {
                return this.ServerIp + GetPathSeparator() + this.StartingPath;
            }
            else
            {
                return this.ServerIp;
            }

        }
        public char GetPathSeparator()
        {
            if(this.PathSeparator!=null && this.PathSeparator!="")//if set
            {
                return Convert.ToChar(this.PathSeparator);
            }
            switch (this.OsType)
            {
                case "windows":
                    return '\\';
                case "linux":
                    return '/';
            }
            return '/';
        }
        public string GetOsNormalizedPath(string fullPath)
        {
            string pathSeparator = GetPathSeparator().ToString();
            if (pathSeparator == "/")
            {
                return fullPath.Replace("\\", pathSeparator);
            }
            else if (pathSeparator == "\\")
            {
                return fullPath.Replace("/", pathSeparator);
            }
            return fullPath;//if no match just return as it is
        }
    }

}
