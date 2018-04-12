using System.Collections.Specialized;

namespace QuartzTelcobright.PropertyGen
{
    public abstract class AbstractQuartzPropertyGenerator
    {
        public NameValueCollection GetSchedulerProperties()
        {
            return GenerateSchedulerProperties();
        }

        protected abstract NameValueCollection GenerateSchedulerProperties();
        
    }
}
