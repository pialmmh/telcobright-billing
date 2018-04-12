using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class ratetaskassignreference:ICacheble<ratetaskassignreference>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{idRatePlan.ToMySqlField()},
				{Description.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<ratetaskassignreference,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<ratetaskassignreference,string> whereClauseMethod)
		{
			return $@"update ratetaskassignreference set 
				id={id.ToMySqlField()+" "},
				idRatePlan={idRatePlan.ToMySqlField()+" "},
				Description={Description.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<ratetaskassignreference,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<ratetaskassignreference,string> whereClauseMethod)
		{
			return $@"delete from ratetaskassignreference 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
