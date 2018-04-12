using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class enumdatedassignment:ICacheble<enumdatedassignment>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{Type.ToMySqlField()},
				{Tuple.ToMySqlField()},
				{description.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<enumdatedassignment,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<enumdatedassignment,string> whereClauseMethod)
		{
			return $@"update enumdatedassignment set 
				id={id.ToMySqlField()+" "},
				Type={Type.ToMySqlField()+" "},
				Tuple={Tuple.ToMySqlField()+" "},
				description={description.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<enumdatedassignment,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<enumdatedassignment,string> whereClauseMethod)
		{
			return $@"delete from enumdatedassignment 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
