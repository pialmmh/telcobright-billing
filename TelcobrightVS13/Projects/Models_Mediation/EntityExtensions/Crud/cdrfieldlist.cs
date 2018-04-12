using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class cdrfieldlist:ICacheble<cdrfieldlist>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{fieldnumber.ToMySqlField()},
				{FieldName.ToMySqlField()},
				{IsNumeric.ToMySqlField()},
				{IsDateTime.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<cdrfieldlist,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<cdrfieldlist,string> whereClauseMethod)
		{
			return $@"update cdrfieldlist set 
				fieldnumber={fieldnumber.ToMySqlField()+" "},
				FieldName={FieldName.ToMySqlField()+" "},
				IsNumeric={IsNumeric.ToMySqlField()+" "},
				IsDateTime={IsDateTime.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<cdrfieldlist,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<cdrfieldlist,string> whereClauseMethod)
		{
			return $@"delete from cdrfieldlist 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
