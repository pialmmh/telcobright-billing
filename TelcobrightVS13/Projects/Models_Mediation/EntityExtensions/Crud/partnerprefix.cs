using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class partnerprefix:ICacheble<partnerprefix>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{idPartner.ToMySqlField()},
				{PrefixType.ToMySqlField()},
				{Prefix.ToMySqlField()},
				{CommonTG.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<partnerprefix,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<partnerprefix,string> whereClauseMethod)
		{
			return $@"update partnerprefix set 
				id={id.ToMySqlField()+" "},
				idPartner={idPartner.ToMySqlField()+" "},
				PrefixType={PrefixType.ToMySqlField()+" "},
				Prefix={Prefix.ToMySqlField()+" "},
				CommonTG={CommonTG.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<partnerprefix,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<partnerprefix,string> whereClauseMethod)
		{
			return $@"delete from partnerprefix 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
