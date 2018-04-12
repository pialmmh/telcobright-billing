using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class enumjobautocreatetype:ICacheble<enumjobautocreatetype>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{Type.ToMySqlField()},
				{Description.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<enumjobautocreatetype,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<enumjobautocreatetype,string> whereClauseMethod)
		{
			return $@"update enumjobautocreatetype set 
				id={id.ToMySqlField()+" "},
				Type={Type.ToMySqlField()+" "},
				Description={Description.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<enumjobautocreatetype,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<enumjobautocreatetype,string> whereClauseMethod)
		{
			return $@"delete from enumjobautocreatetype 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
