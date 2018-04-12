using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class enumpartnerrule:ICacheble<enumpartnerrule>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{Type.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<enumpartnerrule,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<enumpartnerrule,string> whereClauseMethod)
		{
			return $@"update enumpartnerrule set 
				id={id.ToMySqlField()+" "},
				Type={Type.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<enumpartnerrule,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<enumpartnerrule,string> whereClauseMethod)
		{
			return $@"delete from enumpartnerrule 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
