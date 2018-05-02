using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LibraryExtensions;
using MediationModel;
using TelcobrightFileOperations;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Mediation.Cdr;

namespace TelcobrightMediation
{
    public class CdrErasingPreProcessor : CdrReProcessingPreProcessor
    {
        public CdrErasingPreProcessor(CdrCollectorInputData cdrCollectorInputData, List<cdr> finalCdrs)
            : base(cdrCollectorInputData, finalCdrs)
        {
            finalCdrs.ForEach(c => base.NonPartialCdrs.Add(c));
        }

        public override void GetCollectionResults(out CdrCollectionResult newCollectionResult,
            out CdrCollectionResult oldCollectionResult)
        {
            newCollectionResult = null;
            oldCollectionResult = base.CreateOldCollectionResult();
        }
    }
}
