using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class xyzprefix:ICacheble<xyzprefix>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{Prefix.ToMySqlField()},
				{Description.ToMySqlField()},
				{CountryCode.ToMySqlField()},
				{date1.ToMySqlField()},
				{field1.ToMySqlField()},
				{field2.ToMySqlField()},
				{field3.ToMySqlField()},
				{field4.ToMySqlField()},
				{field5.ToMySqlField()},
				{refasr.ToMySqlField()},
				{refacd.ToMySqlField()},
				{refccr.ToMySqlField()},
				{refccrbycc.ToMySqlField()},
				{refpdd.ToMySqlField()},
				{refasrfas.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<xyzprefix,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<xyzprefix,string> whereClauseMethod)
		{
			return $@"update xyzprefix set 
				Prefix={Prefix.ToMySqlField()+" "},
				Description={Description.ToMySqlField()+" "},
				CountryCode={CountryCode.ToMySqlField()+" "},
				date1={date1.ToMySqlField()+" "},
				field1={field1.ToMySqlField()+" "},
				field2={field2.ToMySqlField()+" "},
				field3={field3.ToMySqlField()+" "},
				field4={field4.ToMySqlField()+" "},
				field5={field5.ToMySqlField()+" "},
				refasr={refasr.ToMySqlField()+" "},
				refacd={refacd.ToMySqlField()+" "},
				refccr={refccr.ToMySqlField()+" "},
				refccrbycc={refccrbycc.ToMySqlField()+" "},
				refpdd={refpdd.ToMySqlField()+" "},
				refasrfas={refasrfas.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<xyzprefix,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<xyzprefix,string> whereClauseMethod)
		{
			return $@"delete from xyzprefix 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
