using System;
using System.Collections.Generic;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using MediationModel;
using FlexValidation;
namespace TelcobrightMediation
{
    [Export(typeof(IValidationRule<>))]
    public class UniqueBillIdcannotbeempty : IValidationRule<cdr>
    {
        public string ValidationMessage => "UniqueBillId cannot be empty";
        public bool Validate(cdr obj)
        {
            return !String.IsNullOrEmpty(obj.UniqueBillId) && !String.IsNullOrWhiteSpace(obj.UniqueBillId);
        }
    }
}
