using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation.Helper
{
    public static class TemplateNameHelper
    {
        public static string GetTemplateName(Type type)
        {
            string[] names = type.Namespace.Split('.');
            return $"{names[names.Length - 2]}#{type.Name}";
        }
    }
}
