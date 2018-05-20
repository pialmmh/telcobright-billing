using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace LibraryExtensions
{
    public static class StringBuilderExtensions
    {
        public static Func<int, string> GettabStr = nIndent =>
        {
            StringBuilder tabStr = new StringBuilder();
            for (int i = 1; i <= nIndent; i++)
            {
                tabStr.Append("\t");
            }
            return tabStr.ToString();
        };

        public static StringBuilder AppendWithIndent(this StringBuilder s, string textToAppend, int indent)
        {
            s.Append(GettabStr(indent));
            s.Append(textToAppend);
            return s;
        }

        public static StringBuilder AppendLineWithIndent(this StringBuilder s, string textToAppend, int indent)
        {
            s.Append(GettabStr(indent));
            s.AppendLine(textToAppend);
            return s;
        }
    }
}
