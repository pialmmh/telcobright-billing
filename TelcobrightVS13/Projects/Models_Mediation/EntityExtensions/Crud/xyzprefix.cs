using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class xyzprefix:ICacheble<xyzprefix>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.Prefix.ToMySqlField()).Append(",")
				.Append(this.Description.ToMySqlField()).Append(",")
				.Append(this.CountryCode.ToMySqlField()).Append(",")
				.Append(this.date1.ToMySqlField()).Append(",")
				.Append(this.field1.ToMySqlField()).Append(",")
				.Append(this.field2.ToMySqlField()).Append(",")
				.Append(this.field3.ToMySqlField()).Append(",")
				.Append(this.field4.ToMySqlField()).Append(",")
				.Append(this.field5.ToMySqlField()).Append(",")
				.Append(this.refasr.ToMySqlField()).Append(",")
				.Append(this.refacd.ToMySqlField()).Append(",")
				.Append(this.refccr.ToMySqlField()).Append(",")
				.Append(this.refccrbycc.ToMySqlField()).Append(",")
				.Append(this.refpdd.ToMySqlField()).Append(",")
				.Append(this.refasrfas.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<xyzprefix,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<xyzprefix,string> whereClauseMethod)
		{
			return new StringBuilder("update xyzprefix set ")
				.Append("Prefix=").Append(this.Prefix.ToMySqlField()).Append(",")
				.Append("Description=").Append(this.Description.ToMySqlField()).Append(",")
				.Append("CountryCode=").Append(this.CountryCode.ToMySqlField()).Append(",")
				.Append("date1=").Append(this.date1.ToMySqlField()).Append(",")
				.Append("field1=").Append(this.field1.ToMySqlField()).Append(",")
				.Append("field2=").Append(this.field2.ToMySqlField()).Append(",")
				.Append("field3=").Append(this.field3.ToMySqlField()).Append(",")
				.Append("field4=").Append(this.field4.ToMySqlField()).Append(",")
				.Append("field5=").Append(this.field5.ToMySqlField()).Append(",")
				.Append("refasr=").Append(this.refasr.ToMySqlField()).Append(",")
				.Append("refacd=").Append(this.refacd.ToMySqlField()).Append(",")
				.Append("refccr=").Append(this.refccr.ToMySqlField()).Append(",")
				.Append("refccrbycc=").Append(this.refccrbycc.ToMySqlField()).Append(",")
				.Append("refpdd=").Append(this.refpdd.ToMySqlField()).Append(",")
				.Append("refasrfas=").Append(this.refasrfas.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<xyzprefix,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<xyzprefix,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from xyzprefix 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
