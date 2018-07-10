using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryExtensions
{
    public class ObjectWrapperWithCustomKey<T, TKey> where T : class
    {
        public TKey Key { get; }
        public T Obj { get; }

        public ObjectWrapperWithCustomKey(T obj, Func<T, TKey> keyGenerator)
        {
            Obj = obj;
            this.Key = keyGenerator.Invoke(obj);
        }
    }
}
