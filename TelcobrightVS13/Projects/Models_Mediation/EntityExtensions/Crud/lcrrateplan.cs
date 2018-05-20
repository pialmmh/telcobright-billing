using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class lcrrateplan:ICacheble<lcrrateplan>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.idRatePlan.ToMySqlField()).Append(",")
				.Append(this.Description.ToMySqlField()).Append(",")
				.Append(this.StartDate.ToMySqlField()).Append(",")
				.Append(this.JobCreated.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<lcrrateplan,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<lcrrateplan,string> whereClauseMethod)
		{
			return new StringBuilder("update lcrrateplan set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("idRatePlan=").Append(this.idRatePlan.ToMySqlField()).Append(",")
				.Append("Description=").Append(this.Description.ToMySqlField()).Append(",")
				.Append("StartDate=").Append(this.StartDate.ToMySqlField()).Append(",")
				.Append("JobCreated=").Append(this.JobCreated.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<lcrrateplan,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<lcrrateplan,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from lcrrateplan 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
