using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class lcr:ICacheble<lcr>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.Prefix.ToMySqlField()).Append(",")
				.Append(this.idrateplan.ToMySqlField()).Append(",")
				.Append(this.startdate.ToMySqlField()).Append(",")
				.Append(this.LcrCurrent.ToMySqlField()).Append(",")
				.Append(this.LcrHistory.ToMySqlField()).Append(",")
				.Append(this.LastUpdated.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<lcr,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<lcr,string> whereClauseMethod)
		{
			return new StringBuilder("update lcr set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("Prefix=").Append(this.Prefix.ToMySqlField()).Append(",")
				.Append("idrateplan=").Append(this.idrateplan.ToMySqlField()).Append(",")
				.Append("startdate=").Append(this.startdate.ToMySqlField()).Append(",")
				.Append("LcrCurrent=").Append(this.LcrCurrent.ToMySqlField()).Append(",")
				.Append("LcrHistory=").Append(this.LcrHistory.ToMySqlField()).Append(",")
				.Append("LastUpdated=").Append(this.LastUpdated.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<lcr,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<lcr,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from lcr 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
