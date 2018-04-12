using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class enumvatrule:ICacheble<enumvatrule>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{Type.ToMySqlField()},
				{description.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<enumvatrule,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<enumvatrule,string> whereClauseMethod)
		{
			return $@"update enumvatrule set 
				id={id.ToMySqlField()+" "},
				Type={Type.ToMySqlField()+" "},
				description={description.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<enumvatrule,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<enumvatrule,string> whereClauseMethod)
		{
			return $@"delete from enumvatrule 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
