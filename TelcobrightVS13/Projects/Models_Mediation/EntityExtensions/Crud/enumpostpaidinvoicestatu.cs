using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class enumpostpaidinvoicestatu:ICacheble<enumpostpaidinvoicestatu>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{Type.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<enumpostpaidinvoicestatu,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<enumpostpaidinvoicestatu,string> whereClauseMethod)
		{
			return $@"update enumpostpaidinvoicestatus set 
				id={id.ToMySqlField()+" "},
				Type={Type.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<enumpostpaidinvoicestatu,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<enumpostpaidinvoicestatu,string> whereClauseMethod)
		{
			return $@"delete from enumpostpaidinvoicestatus 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
