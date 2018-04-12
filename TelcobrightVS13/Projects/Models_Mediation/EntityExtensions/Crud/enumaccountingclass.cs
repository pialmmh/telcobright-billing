using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class enumaccountingclass:ICacheble<enumaccountingclass>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{Name.ToMySqlField()},
				{NormalBalance.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<enumaccountingclass,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<enumaccountingclass,string> whereClauseMethod)
		{
			return $@"update enumaccountingclass set 
				id={id.ToMySqlField()+" "},
				Name={Name.ToMySqlField()+" "},
				NormalBalance={NormalBalance.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<enumaccountingclass,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<enumaccountingclass,string> whereClauseMethod)
		{
			return $@"delete from enumaccountingclass 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
