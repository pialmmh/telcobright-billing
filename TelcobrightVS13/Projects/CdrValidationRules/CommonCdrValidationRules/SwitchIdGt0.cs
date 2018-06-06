﻿using System;
using System.ComponentModel.Composition;
using MediationModel;
using TelcobrightMediation;

namespace CdrValidationRules
{
    [Export(typeof(IValidationRule<>))]
    public class SwitchIdGt0 : IValidationRule<cdr>
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public string ValidationMessage => "SwitchId must be > 0";
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
            return obj.SwitchId > 0;
        }
    }
}

