using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class enumcdrformat:ICacheble<enumcdrformat>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{Type.ToMySqlField()},
				{FieldSeparatorTxt.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<enumcdrformat,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<enumcdrformat,string> whereClauseMethod)
		{
			return $@"update enumcdrformat set 
				id={id.ToMySqlField()+" "},
				Type={Type.ToMySqlField()+" "},
				FieldSeparatorTxt={FieldSeparatorTxt.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<enumcdrformat,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<enumcdrformat,string> whereClauseMethod)
		{
			return $@"delete from enumcdrformat 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
