using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class cdrpartialreference:ICacheble<cdrpartialreference>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{UniqueBillId.ToMySqlField()},
				{switchid.ToMySqlField()},
				{lastIdcall.ToMySqlField()},
				{CallDate.ToMySqlField()},
				{commaSepIdcallsForAllInstances.ToMySqlField()},
				{lastFilename.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<cdrpartialreference,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<cdrpartialreference,string> whereClauseMethod)
		{
			return $@"update cdrpartialreference set 
				UniqueBillId={UniqueBillId.ToMySqlField()+" "},
				switchid={switchid.ToMySqlField()+" "},
				lastIdcall={lastIdcall.ToMySqlField()+" "},
				CallDate={CallDate.ToMySqlField()+" "},
				commaSepIdcallsForAllInstances={commaSepIdcallsForAllInstances.ToMySqlField()+" "},
				lastFilename={lastFilename.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<cdrpartialreference,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<cdrpartialreference,string> whereClauseMethod)
		{
			return $@"delete from cdrpartialreference 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
