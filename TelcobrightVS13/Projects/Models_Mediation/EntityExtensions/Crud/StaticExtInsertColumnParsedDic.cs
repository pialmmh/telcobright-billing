using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public static class StaticExtInsertColumnParsedDic
	{
		private static readonly Dictionary<string,string> _parsedInsertHeaders=new Dictionary<string, string>();
	    public static bool IsParsed { get; set; } = false;
		public static void Parse()
		{
		    if (IsParsed == true) return;
            Type t = typeof(StaticExtInsertColumnHeaders);
			foreach (var propertyInfo in t.GetProperties())
			{
			    if (_parsedInsertHeaders.ContainsKey(propertyInfo.Name)==false)//this static class can be called from multiple process or thread, populating only once is enough
			    {
			        _parsedInsertHeaders.Add(propertyInfo.Name,propertyInfo.GetValue(null).ToString());
                }
            }
			IsParsed = true;
		}

		public static Dictionary<string, string> GetParsedDic()
		{
			if(IsParsed==false) Parse();
                //throw new Exception("Static properties are not parsed to dictionary, method 'Parse()' must be called once before using this property.");
			return _parsedInsertHeaders;
		}
	}
}
