using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class ratetaskreference:ICacheble<ratetaskreference>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{idRatePlan.ToMySqlField()},
				{Description.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<ratetaskreference,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<ratetaskreference,string> whereClauseMethod)
		{
			return $@"update ratetaskreference set 
				id={id.ToMySqlField()+" "},
				idRatePlan={idRatePlan.ToMySqlField()+" "},
				Description={Description.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<ratetaskreference,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<ratetaskreference,string> whereClauseMethod)
		{
			return $@"delete from ratetaskreference 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
