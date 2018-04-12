using System.Threading.Tasks;

namespace TelcobrightMediation
{
    public abstract class AbstractListener
    {
        public abstract EventListenerType ListenerType { get; }
        public abstract Task Execute(object listenerData);
    }
}
