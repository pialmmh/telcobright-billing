using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryExtensions
{
    public interface IValidationRule<T>
    {
        object Data { get; set; }
        void Prepare();
        bool Validate(T obj);
        string ValidationMessage { get; }
    }
}
