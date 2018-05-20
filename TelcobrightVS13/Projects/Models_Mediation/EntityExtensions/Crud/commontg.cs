using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class commontg:ICacheble<commontg>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.TgName.ToMySqlField()).Append(",")
				.Append(this.idSwitch.ToMySqlField()).Append(",")
				.Append(this.description.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<commontg,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<commontg,string> whereClauseMethod)
		{
			return new StringBuilder("update commontg set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("TgName=").Append(this.TgName.ToMySqlField()).Append(",")
				.Append("idSwitch=").Append(this.idSwitch.ToMySqlField()).Append(",")
				.Append("description=").Append(this.description.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<commontg,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<commontg,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from commontg 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
