using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class bridgedroute:ICacheble<bridgedroute>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.switchId.ToMySqlField()).Append(",")
				.Append(this.routeName.ToMySqlField()).Append(",")
				.Append(this.inPartner.ToMySqlField()).Append(",")
				.Append(this.outPartner.ToMySqlField()).Append(",")
				.Append(this.nationalOrInternational.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<bridgedroute,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<bridgedroute,string> whereClauseMethod)
		{
			return new StringBuilder("update bridgedroute set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("switchId=").Append(this.switchId.ToMySqlField()).Append(",")
				.Append("routeName=").Append(this.routeName.ToMySqlField()).Append(",")
				.Append("inPartner=").Append(this.inPartner.ToMySqlField()).Append(",")
				.Append("outPartner=").Append(this.outPartner.ToMySqlField()).Append(",")
				.Append("nationalOrInternational=").Append(this.nationalOrInternational.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<bridgedroute,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<bridgedroute,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from bridgedroute 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
