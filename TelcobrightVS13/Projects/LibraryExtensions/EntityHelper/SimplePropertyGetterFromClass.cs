using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LibraryExtensions.CommonHelper
{
    public class ClassTypeOrPropertyHelper
    {
        public List<string> GetSimplePropertyNames(Type t)
        {
            return t.GetProperties().Where(p => !p.PropertyType.IsClass
                                                || p.PropertyType == typeof(String))
                .Select(p => p.Name).ToList();
        }
    }
}
