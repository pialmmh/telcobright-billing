using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibraryExtensions
{
    public static class StringBuilderJoiner
    {
        //using parallel is not very rewarding as locking makes it behave like single thread
        public static StringBuilder Join(string delimiter, IEnumerable<StringBuilder> sbs)
        {
            if (!sbs.Any()) return new StringBuilder();
            StringBuilder sbFirst = sbs.First();
            IEnumerable<StringBuilder> restOfTheSbsExceptFirst = sbs.Skip(1);
            foreach (var sbNext in restOfTheSbsExceptFirst)
            {
                sbFirst.Append(delimiter).Append(sbNext);
            }
            return sbFirst;
        }
    }
}