using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class enumratesheetformat:ICacheble<enumratesheetformat>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{Type.ToMySqlField()},
				{IdentifierTextJson.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<enumratesheetformat,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<enumratesheetformat,string> whereClauseMethod)
		{
			return $@"update enumratesheetformat set 
				id={id.ToMySqlField()+" "},
				Type={Type.ToMySqlField()+" "},
				IdentifierTextJson={IdentifierTextJson.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<enumratesheetformat,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<enumratesheetformat,string> whereClauseMethod)
		{
			return $@"delete from enumratesheetformat 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
