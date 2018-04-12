using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class enumprepaidinvoicestatu:ICacheble<enumprepaidinvoicestatu>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{Type.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<enumprepaidinvoicestatu,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<enumprepaidinvoicestatu,string> whereClauseMethod)
		{
			return $@"update enumprepaidinvoicestatus set 
				id={id.ToMySqlField()+" "},
				Type={Type.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<enumprepaidinvoicestatu,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<enumprepaidinvoicestatu,string> whereClauseMethod)
		{
			return $@"delete from enumprepaidinvoicestatus 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
