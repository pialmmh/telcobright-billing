using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class lcr:ICacheble<lcr>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{Prefix.ToMySqlField()},
				{idrateplan.ToMySqlField()},
				{startdate.ToMySqlField()},
				{LcrCurrent.ToMySqlField()},
				{LcrHistory.ToMySqlField()},
				{LastUpdated.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<lcr,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<lcr,string> whereClauseMethod)
		{
			return $@"update lcr set 
				id={id.ToMySqlField()+" "},
				Prefix={Prefix.ToMySqlField()+" "},
				idrateplan={idrateplan.ToMySqlField()+" "},
				startdate={startdate.ToMySqlField()+" "},
				LcrCurrent={LcrCurrent.ToMySqlField()+" "},
				LcrHistory={LcrHistory.ToMySqlField()+" "},
				LastUpdated={LastUpdated.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<lcr,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<lcr,string> whereClauseMethod)
		{
			return $@"delete from lcr 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
