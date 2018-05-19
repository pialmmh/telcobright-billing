using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class jobcompletion:ICacheble<jobcompletion>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.idJob.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<jobcompletion,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<jobcompletion,string> whereClauseMethod)
		{
			return new StringBuilder("update jobcompletion set ")
				.Append("idJob=").Append(this.idJob.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<jobcompletion,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<jobcompletion,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from jobcompletion 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
