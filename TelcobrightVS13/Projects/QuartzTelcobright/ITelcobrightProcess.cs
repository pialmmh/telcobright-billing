using Quartz;
namespace QuartzTelcobright
{
    public interface ITelcobrightProcess:IJob
    {
        //string RuleName { get; }
        string HelpText { get; }
        int ProcessId { get; }
        //void Execute(IJobExecutionContext schedulerContext);
        //execute not required as it is forced by quartz.IJob
    }
}