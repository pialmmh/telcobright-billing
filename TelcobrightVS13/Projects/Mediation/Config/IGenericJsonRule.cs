using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelcobrightMediation.Config;
namespace TelcobrightMediation.Accounting
{
    public interface IGenericJsonRule
    {
        int Id { get; }
        string RuleName { get; }
        string Description { get; }
        Dictionary<string, string> JsonParameters { get; set; }
    }
}
