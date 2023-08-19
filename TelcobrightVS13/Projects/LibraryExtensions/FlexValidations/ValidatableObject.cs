using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spring.Core.TypeResolution;
using Spring.Expressions;
namespace LibraryExtensions
{
    public static class Parsers
    {
        public static Dictionary<string,Func<string,DateTime>> DateParsers { get; set; }
        public static Dictionary<string, Func<string, double>> DoubleParsers { get; set; }
    }
    public class ValidatableObject<T>
    {
        private readonly T obj;
        private FlexValidator<T> Validator { get; }
        public ValidatableObject(T obj,FlexValidator<T> flexValidator)
        {
            //TypeRegistry.RegisterType("Parsers", typeof(FlexValidation.Parsers));
            this.obj = obj;
            this.Validator = flexValidator;
        }
    }
}
