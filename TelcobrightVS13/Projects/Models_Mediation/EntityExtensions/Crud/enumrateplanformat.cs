using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class enumrateplanformat:ICacheble<enumrateplanformat>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{Type.ToMySqlField()},
				{Description.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<enumrateplanformat,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<enumrateplanformat,string> whereClauseMethod)
		{
			return $@"update enumrateplanformat set 
				id={id.ToMySqlField()+" "},
				Type={Type.ToMySqlField()+" "},
				Description={Description.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<enumrateplanformat,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<enumrateplanformat,string> whereClauseMethod)
		{
			return $@"delete from enumrateplanformat 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
