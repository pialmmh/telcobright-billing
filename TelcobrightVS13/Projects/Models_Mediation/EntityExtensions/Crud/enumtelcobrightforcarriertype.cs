using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class enumtelcobrightforcarriertype:ICacheble<enumtelcobrightforcarriertype>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{Type.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<enumtelcobrightforcarriertype,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<enumtelcobrightforcarriertype,string> whereClauseMethod)
		{
			return $@"update enumtelcobrightforcarriertype set 
				id={id.ToMySqlField()+" "},
				Type={Type.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<enumtelcobrightforcarriertype,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<enumtelcobrightforcarriertype,string> whereClauseMethod)
		{
			return $@"delete from enumtelcobrightforcarriertype 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
