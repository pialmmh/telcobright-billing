using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CdrValidationRules
{
    using System;
    using System.ComponentModel.Composition;
    using MediationModel;
    using TelcobrightMediation;

    namespace CdrValidationRules.CommonCdrValidationRules
    {
        [Export(typeof(IValidationRule<>))]
        public class FileNameNotEmpty : IValidationRule<cdr>
        {
            public override string ToString() => this.RuleName;
            public string RuleName => GetType().Name;
            public string ValidationMessage => "FileName cannot be empty";
            public object Data { get; set; }
            public bool IsPrepared { get; private set; }

            public void Prepare()
            {
                this.IsPrepared = true;
            }

            public bool Validate(cdr obj)
            {
                if (this.IsPrepared == false)
                    throw new Exception("Rule is not prepared, method Prepare must be called first.");
                return !String.IsNullOrEmpty(obj.FileName) && !String.IsNullOrWhiteSpace(obj.FileName);
            }
        }
    }
}
