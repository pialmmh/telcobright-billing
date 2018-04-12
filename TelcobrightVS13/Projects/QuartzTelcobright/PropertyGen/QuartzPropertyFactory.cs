using System.Collections.Specialized;
using System.Configuration;
using LibraryExtensions.ConfigHelper;

namespace QuartzTelcobright.PropertyGen
{
    public class QuartzPropertyFactory
    {
        public AbstractQuartzPropertyGenerator AbstractQuartzPropertyGenerator { get; }
        public QuartzPropertyFactory(AbstractQuartzPropertyGenerator abstractQuartzPropertyGenerator)
        {
            this.AbstractQuartzPropertyGenerator = abstractQuartzPropertyGenerator;
        }
        public NameValueCollection GetProperties()
        {
            return this.AbstractQuartzPropertyGenerator.GetSchedulerProperties();
        }
    }
}
