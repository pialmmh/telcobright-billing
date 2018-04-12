using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class lcrrateplan:ICacheble<lcrrateplan>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{idRatePlan.ToMySqlField()},
				{Description.ToMySqlField()},
				{StartDate.ToMySqlField()},
				{JobCreated.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<lcrrateplan,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<lcrrateplan,string> whereClauseMethod)
		{
			return $@"update lcrrateplan set 
				id={id.ToMySqlField()+" "},
				idRatePlan={idRatePlan.ToMySqlField()+" "},
				Description={Description.ToMySqlField()+" "},
				StartDate={StartDate.ToMySqlField()+" "},
				JobCreated={JobCreated.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<lcrrateplan,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<lcrrateplan,string> whereClauseMethod)
		{
			return $@"delete from lcrrateplan 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
