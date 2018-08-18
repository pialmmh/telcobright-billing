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
    public class CdrErrorPreProcessor : AbstractCdrReProcessingPreProcessor
    {
        public CdrErrorPreProcessor(CdrCollectorInputData cdrCollectorInputData, List<cdr> finalCdrs)
            : base(cdrCollectorInputData, finalCdrs)
        {
        }

        public override void GetCollectionResults(out CdrCollectionResult newCollectionResult,
            out CdrCollectionResult oldCollectionResult)
        {
            newCollectionResult = CreateNewCollectionResult();
            oldCollectionResult = null;
        }
        protected override List<CdrExt> CreateOldCdrExts()
        {
            throw new NotImplementedException();
        }
        protected override CdrCollectionResult CreateOldCollectionResult()
        {
            throw new NotImplementedException();
        }
    }
}
