using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class cdrpartialreference:ICacheble<cdrpartialreference>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.UniqueBillId.ToMySqlField()).Append(",")
				.Append(this.switchid.ToMySqlField()).Append(",")
				.Append(this.lastIdcall.ToMySqlField()).Append(",")
				.Append(this.CallDate.ToMySqlField()).Append(",")
				.Append(this.commaSepIdcallsForAllInstances.ToMySqlField()).Append(",")
				.Append(this.lastFilename.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<cdrpartialreference,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<cdrpartialreference,string> whereClauseMethod)
		{
			return new StringBuilder("update cdrpartialreference set ")
				.Append("UniqueBillId=").Append(this.UniqueBillId.ToMySqlField()).Append(",")
				.Append("switchid=").Append(this.switchid.ToMySqlField()).Append(",")
				.Append("lastIdcall=").Append(this.lastIdcall.ToMySqlField()).Append(",")
				.Append("CallDate=").Append(this.CallDate.ToMySqlField()).Append(",")
				.Append("commaSepIdcallsForAllInstances=").Append(this.commaSepIdcallsForAllInstances.ToMySqlField()).Append(",")
				.Append("lastFilename=").Append(this.lastFilename.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<cdrpartialreference,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<cdrpartialreference,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from cdrpartialreference 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
