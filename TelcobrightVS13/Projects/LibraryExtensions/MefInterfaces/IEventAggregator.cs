using System.Collections.Generic;
using MediationModel;

namespace LibraryExtensions
{
    public interface IEventAggregator<T>
    {
        string RuleName { get; }
        string HelpText { get; }
        int Id { get; }
        T GetAggregatedSingleInstance(List<T> oldInstances, List<T> newInstances);
    }
}