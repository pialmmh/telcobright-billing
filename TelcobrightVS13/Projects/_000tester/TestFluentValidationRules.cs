﻿using System.Collections.Generic;
using MediationModel;

namespace Utils
{
    public class TestFluentValidationRules:AbstractFluentValidationTester
    {
        public override void Test()
        {
            
        }

        public TestFluentValidationRules(List<cdrfieldlist> cdrfieldlists) : base(cdrfieldlists)
        {
        }
    }
}