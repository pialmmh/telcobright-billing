using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Newtonsoft.Json;
namespace Utils
{
    public class CdrValidator : AbstractValidator<cdr>
    {
        public CdrValidator()
        {
            var refdate=new DateTime(2000,1,1);
            RuleFor(cdr => cdr.StartTime).GreaterThanOrEqualTo(refdate);
            RuleFor(cdr => cdr.OriginatingCalledNumber).NotEmpty();
        }
    }
    class FluentTester
    {
        public void Test()
        {
            CdrValidator validator = new CdrValidator();
            string valStr = JsonConvert.SerializeObject(validator);
            CdrValidator validator2 = JsonConvert.DeserializeObject<CdrValidator>(valStr);
            List<cdr> cdrs=new List<cdr>();
            using (PartnerEntities context=new PartnerEntities())
            {
                cdrs = context.cdrs.Take(20000).ToList();
            }
            
            cdrs.ForEach(c =>
            {
                ValidationResult result = validator.Validate(c);
            });

            cdrs.ForEach(c =>
            {
                ValidationResult result = validator2.Validate(c);
            });
        }
    }
}
