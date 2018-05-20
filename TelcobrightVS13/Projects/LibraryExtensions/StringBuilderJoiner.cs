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
            var strinbBuilders = sbs.ToList();
            if (!strinbBuilders.Any()) return new StringBuilder();
            StringBuilder sbFirst = strinbBuilders.First();
            foreach (var sbNext in strinbBuilders.Skip(1))
            {
                sbFirst.Append(delimiter).Append(sbNext);
            }
            return sbFirst;
        }
    }
}