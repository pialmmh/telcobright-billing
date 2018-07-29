using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class autoinc_manual_long:ICacheble<autoinc_manual_long>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.Id.ToMySqlField()).Append(",")
				.Append(this.incrementRequestedOn.ToMySqlField()).Append(",")
				.Append(this.randomValue.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<autoinc_manual_long,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<autoinc_manual_long,string> whereClauseMethod)
		{
			return new StringBuilder("update autoinc_manual_long set ")
				.Append("Id=").Append(this.Id.ToMySqlField()).Append(",")
				.Append("incrementRequestedOn=").Append(this.incrementRequestedOn.ToMySqlField()).Append(",")
				.Append("randomValue=").Append(this.randomValue.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<autoinc_manual_long,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<autoinc_manual_long,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from autoinc_manual_long 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
