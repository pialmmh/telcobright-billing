using System;
namespace InstallConfig
{
    public class TelcobrightSeed{
        private int schedulerPortNo = 555;
        public string getNextSchedulerPort()
        {
            return (++schedulerPortNo).ToString();
        }
    }
}