using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation
{
    public interface ISegmentedJob
    {
        void Execute();
        int ActualStepsCount { get; }
    }
}
