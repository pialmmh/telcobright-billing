using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class reporttemplate:ICacheble<reporttemplate>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{Templatename.ToMySqlField()},
				{PageUrl.ToMySqlField()},
				{ControlValues.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<reporttemplate,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<reporttemplate,string> whereClauseMethod)
		{
			return $@"update reporttemplate set 
				Templatename={Templatename.ToMySqlField()+" "},
				PageUrl={PageUrl.ToMySqlField()+" "},
				ControlValues={ControlValues.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<reporttemplate,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<reporttemplate,string> whereClauseMethod)
		{
			return $@"delete from reporttemplate 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
