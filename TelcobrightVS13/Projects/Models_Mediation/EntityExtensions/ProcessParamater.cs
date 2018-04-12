namespace MediationModel
{
    public class ProcessParamater
    {
        public int TimerInterval { get; set; }//use int, thread doesn't take  long
        public string MainMethodName { get; set; }
        public ProcessParamater(int timerInterval)
        {
            this.TimerInterval = timerInterval;
        }
    }
}