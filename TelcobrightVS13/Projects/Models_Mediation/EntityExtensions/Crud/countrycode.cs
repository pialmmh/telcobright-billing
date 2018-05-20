using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class countrycode:ICacheble<countrycode>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.Code.ToMySqlField()).Append(",")
				.Append(this.Name.ToMySqlField()).Append(",")
				.Append(this.refasr.ToMySqlField()).Append(",")
				.Append(this.refacd.ToMySqlField()).Append(",")
				.Append(this.refccr.ToMySqlField()).Append(",")
				.Append(this.refccrbycc.ToMySqlField()).Append(",")
				.Append(this.refpdd.ToMySqlField()).Append(",")
				.Append(this.refasrfas.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<countrycode,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<countrycode,string> whereClauseMethod)
		{
			return new StringBuilder("update countrycode set ")
				.Append("Code=").Append(this.Code.ToMySqlField()).Append(",")
				.Append("Name=").Append(this.Name.ToMySqlField()).Append(",")
				.Append("refasr=").Append(this.refasr.ToMySqlField()).Append(",")
				.Append("refacd=").Append(this.refacd.ToMySqlField()).Append(",")
				.Append("refccr=").Append(this.refccr.ToMySqlField()).Append(",")
				.Append("refccrbycc=").Append(this.refccrbycc.ToMySqlField()).Append(",")
				.Append("refpdd=").Append(this.refpdd.ToMySqlField()).Append(",")
				.Append("refasrfas=").Append(this.refasrfas.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<countrycode,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<countrycode,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from countrycode 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
