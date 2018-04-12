using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class enumservicefamily:ICacheble<enumservicefamily>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{ServiceName.ToMySqlField()},
				{Description.ToMySqlField()},
				{PartnerAssignNotNeeded.ToMySqlField()},
				{ServiceCategory.ToMySqlField()},
				{AccountingId.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<enumservicefamily,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<enumservicefamily,string> whereClauseMethod)
		{
			return $@"update enumservicefamily set 
				id={id.ToMySqlField()+" "},
				ServiceName={ServiceName.ToMySqlField()+" "},
				Description={Description.ToMySqlField()+" "},
				PartnerAssignNotNeeded={PartnerAssignNotNeeded.ToMySqlField()+" "},
				ServiceCategory={ServiceCategory.ToMySqlField()+" "},
				AccountingId={AccountingId.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<enumservicefamily,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<enumservicefamily,string> whereClauseMethod)
		{
			return $@"delete from enumservicefamily 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
