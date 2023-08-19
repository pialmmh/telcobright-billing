using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spring.Expressions;
namespace LibraryExtensions
{
    public static class Conditions
    {
        //public static bool When(string expression,object objectContext)
        //{
        //    var instance = Convert.ChangeType(objectContext, objectContext.GetType());
        //    IExpression exp = Expression.Parse(expression);
        //    var x=(bool)exp.GetValue(instance);
        //    return x;
        //}
        public static bool When(string expression)
        {
            IExpression exp = Expression.Parse(expression);
            var x = (bool)exp.GetValue(expression);
            return x;
        }
    }
}
