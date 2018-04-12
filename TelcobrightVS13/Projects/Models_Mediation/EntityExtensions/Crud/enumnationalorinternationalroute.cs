using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class enumnationalorinternationalroute:ICacheble<enumnationalorinternationalroute>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{Type.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<enumnationalorinternationalroute,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<enumnationalorinternationalroute,string> whereClauseMethod)
		{
			return $@"update enumnationalorinternationalroute set 
				id={id.ToMySqlField()+" "},
				Type={Type.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<enumnationalorinternationalroute,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<enumnationalorinternationalroute,string> whereClauseMethod)
		{
			return $@"delete from enumnationalorinternationalroute 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
