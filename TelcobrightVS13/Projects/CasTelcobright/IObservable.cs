using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CasTelcobright
{
    public interface IObservable
    {
        void subscribe(IObserver observer);  // could be used by the observer with the observable
        void Notify();
    }
}
