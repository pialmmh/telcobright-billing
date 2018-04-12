using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class product:ICacheble<product>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{Prefix.ToMySqlField()},
				{Name.ToMySqlField()},
				{description.ToMySqlField()},
				{Category.ToMySqlField()},
				{SubCategory.ToMySqlField()},
				{ServiceFamily.ToMySqlField()},
				{AccountingId.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<product,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<product,string> whereClauseMethod)
		{
			return $@"update product set 
				id={id.ToMySqlField()+" "},
				Prefix={Prefix.ToMySqlField()+" "},
				Name={Name.ToMySqlField()+" "},
				description={description.ToMySqlField()+" "},
				Category={Category.ToMySqlField()+" "},
				SubCategory={SubCategory.ToMySqlField()+" "},
				ServiceFamily={ServiceFamily.ToMySqlField()+" "},
				AccountingId={AccountingId.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<product,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<product,string> whereClauseMethod)
		{
			return $@"delete from product 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
