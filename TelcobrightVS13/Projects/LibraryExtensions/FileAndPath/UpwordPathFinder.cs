using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LibraryExtensions
{
    public class UpwordPathFinder<T>
    {
        public DirectoryInfo SearchBeginsAt { get; private set; }
        public string FileOrFolderNameToFind { get; private set; }
        public UpwordPathFinder(string fileOrFolderNameToFind)
        {
            string searchBeginsAt = FileAndPathHelperReadOnly.GetCurrentExecPath();
            Init(fileOrFolderNameToFind, searchBeginsAt);
        }

        public UpwordPathFinder(string fileOrFolderNameToFind, string searchBeginsAt)
        {
            if (searchBeginsAt.IsNullOrEmptyOrWhiteSpace())
                throw new Exception("Search begins at cannot be passed as empty, call the other overload if search begins in the the current exec path.");
            Init(fileOrFolderNameToFind, searchBeginsAt);
        }

        private void Init(string fileOrFolderNameToFind, string searchBeginsAt)
        {
            if (typeof(T) != typeof(FileInfo) && typeof(T) != typeof(DirectoryInfo))
            {
                throw new InvalidConstraintException("Target must be FileInfo or DirectoryInfo");
            }
            this.SearchBeginsAt = new DirectoryInfo(searchBeginsAt);
            this.FileOrFolderNameToFind = fileOrFolderNameToFind;
        }

        public string FindAndGetFullPath()
        {
            DirectoryInfo possibleTargetDir = new DirectoryInfo(FileAndPathHelperReadOnly.GetCurrentExecPath());
            while (!Directory.Exists(Path.Combine(possibleTargetDir.FullName, this.FileOrFolderNameToFind)))
            {
                possibleTargetDir = possibleTargetDir.Parent;
            }
            return Path.Combine(possibleTargetDir.FullName, FileOrFolderNameToFind);
        }
    }
}
