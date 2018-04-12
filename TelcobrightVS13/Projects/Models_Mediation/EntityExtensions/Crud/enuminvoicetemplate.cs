using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class enuminvoicetemplate:ICacheble<enuminvoicetemplate>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{TemplateName.ToMySqlField()},
				{Description.ToMySqlField()},
				{json.ToMySqlField()},
				{OtherInfo.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<enuminvoicetemplate,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<enuminvoicetemplate,string> whereClauseMethod)
		{
			return $@"update enuminvoicetemplate set 
				id={id.ToMySqlField()+" "},
				TemplateName={TemplateName.ToMySqlField()+" "},
				Description={Description.ToMySqlField()+" "},
				json={json.ToMySqlField()+" "},
				OtherInfo={OtherInfo.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<enuminvoicetemplate,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<enuminvoicetemplate,string> whereClauseMethod)
		{
			return $@"delete from enuminvoicetemplate 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
