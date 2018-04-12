using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class enumservicegroup:ICacheble<enumservicegroup>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{Type.ToMySqlField()},
				{AccountingId.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<enumservicegroup,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<enumservicegroup,string> whereClauseMethod)
		{
			return $@"update enumservicegroup set 
				id={id.ToMySqlField()+" "},
				Type={Type.ToMySqlField()+" "},
				AccountingId={AccountingId.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<enumservicegroup,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<enumservicegroup,string> whereClauseMethod)
		{
			return $@"delete from enumservicegroup 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
