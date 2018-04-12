using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation.Cdr
{
    public class CdrJob : ISegmentedJob
    {
        public CdrProcessor CdrProcessor { get; }
        public CdrEraser CdrEraser { get; }
        public int ActualStepsCount { get; }
        public CdrJobContext CdrJobContext { get; }

        public CdrJob(CdrProcessor cdrProcessor, CdrEraser cdrEraser, int actualStepsCount)
        {
            this.CdrJobContext = cdrProcessor?.CdrJobContext ?? cdrEraser.CdrJobContext;
            this.CdrProcessor = cdrProcessor;
            this.CdrEraser = cdrEraser;
            this.ActualStepsCount = actualStepsCount;
        }

        public void Execute()
        {
            this.CdrEraser?.Process();
            this.CdrEraser?.WriteChangesExceptContext();

            this.CdrProcessor?.Process();
            this.CdrProcessor?.WriteChangesExceptContext();
            this.CdrJobContext.WriteChanges();
        }
    }
}
