using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class errordefinition:ICacheble<errordefinition>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{idError.ToMySqlField()},
				{Description.ToMySqlField()},
				{Severity.ToMySqlField()},
				{Action.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<errordefinition,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<errordefinition,string> whereClauseMethod)
		{
			return $@"update errordefinition set 
				idError={idError.ToMySqlField()+" "},
				Description={Description.ToMySqlField()+" "},
				Severity={Severity.ToMySqlField()+" "},
				Action={Action.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<errordefinition,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<errordefinition,string> whereClauseMethod)
		{
			return $@"delete from errordefinition 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
