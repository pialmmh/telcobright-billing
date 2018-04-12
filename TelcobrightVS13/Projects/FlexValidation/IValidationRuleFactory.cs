using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlexValidation;

namespace TelcobrightMediation.Config
{
    public interface IValidationRuleFactory
    {
        ValidationRule CreateValidationRule(KeyValuePair<string, string> validationExpressionsWithMessages);
    }
}
