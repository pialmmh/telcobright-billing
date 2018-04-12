using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class countrycode:ICacheble<countrycode>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{Code.ToMySqlField()},
				{Name.ToMySqlField()},
				{refasr.ToMySqlField()},
				{refacd.ToMySqlField()},
				{refccr.ToMySqlField()},
				{refccrbycc.ToMySqlField()},
				{refpdd.ToMySqlField()},
				{refasrfas.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<countrycode,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<countrycode,string> whereClauseMethod)
		{
			return $@"update countrycode set 
				Code={Code.ToMySqlField()+" "},
				Name={Name.ToMySqlField()+" "},
				refasr={refasr.ToMySqlField()+" "},
				refacd={refacd.ToMySqlField()+" "},
				refccr={refccr.ToMySqlField()+" "},
				refccrbycc={refccrbycc.ToMySqlField()+" "},
				refpdd={refpdd.ToMySqlField()+" "},
				refasrfas={refasrfas.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<countrycode,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<countrycode,string> whereClauseMethod)
		{
			return $@"delete from countrycode 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
