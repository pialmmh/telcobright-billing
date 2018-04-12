using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class commontg:ICacheble<commontg>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{TgName.ToMySqlField()},
				{idSwitch.ToMySqlField()},
				{description.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<commontg,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<commontg,string> whereClauseMethod)
		{
			return $@"update commontg set 
				id={id.ToMySqlField()+" "},
				TgName={TgName.ToMySqlField()+" "},
				idSwitch={idSwitch.ToMySqlField()+" "},
				description={description.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<commontg,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<commontg,string> whereClauseMethod)
		{
			return $@"delete from commontg 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
