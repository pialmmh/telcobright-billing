using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;

namespace Utils
{
    public enum FluentTestingType
    {
        RuleBased,
        RuleSetBased
    }
    public abstract class  AbstractFluentValidationTester
    {
        List<cdrfieldlist> fields { get; set; }

        protected AbstractFluentValidationTester(List<cdrfieldlist> cdrfieldlists)
        {
            this.fields = cdrfieldlists;
        }

        public abstract void Test();
        public void Test(FluentTestingType fluentTestingType, AbstractValidator<cdrfieldlist> validator)
        {
            List<ValidationResult> results = new List<ValidationResult>();
            int counter = 0;
            int totalIteration = 100000;
            int feedbackInterval = 10000;
            int segmentCounter = 0;
            ValidationResult result = null;
            Console.Write(" RuleSegments: ");
            for (var index = 0; index < fields.Count; index++)
            {
                var cdrfieldlist = fields[index];
                switch (fluentTestingType)
                {
                    case FluentTestingType.RuleBased:
                        result = validator.Validate(cdrfieldlist);
                        break;
                    case FluentTestingType.RuleSetBased:
                        result = validator.Validate(cdrfieldlist,
                            new RulesetValidatorSelector("rsFieldName", "rsIsNumeric", "rsFieldNumber"));
                        break;
                }
                results.Add(result);
                counter++;
                if (counter % feedbackInterval == 0)
                {
                    segmentCounter++;
                    Console.Write(segmentCounter + " ");
                }
                if (counter >= totalIteration) break;
                if (index == 102)
                {
                    index = 0;
                }
            }
        }
    }
}