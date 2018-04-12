using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexValidation
{
    public interface IFlexValidator<T> 
    {
        ValidationResult Validate(T instance);
    }
}
