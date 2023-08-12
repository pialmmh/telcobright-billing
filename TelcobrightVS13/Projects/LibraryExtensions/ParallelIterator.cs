using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryExtensions
{
    public class ParallelIterator<TInput,TOutput>
    {
        readonly TInput[] _inputAsArr=null;
        public ParallelIterator(IEnumerable<TInput> collection)
        {
            this._inputAsArr = collection.ToArray();
        }
        public List<TOutput> getOutput(Func<TInput,TOutput> methodToIterateForEachItem)
        {
            int count = _inputAsArr.Length;
            TOutput[] outputs = new TOutput[count];
            Parallel.For(0, count, index => {
                TOutput result = methodToIterateForEachItem(_inputAsArr[index]);
                outputs[index] = result; // No need of a concurrent collection to store the result!
            });
            return outputs.ToList();
        }
    }
}
