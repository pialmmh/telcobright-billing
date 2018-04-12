using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class partner:ICacheble<partner>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{idPartner.ToMySqlField()},
				{PartnerName.ToMySqlField()},
				{Address1.ToMySqlField()},
				{Address2.ToMySqlField()},
				{City.ToMySqlField()},
				{State.ToMySqlField()},
				{PostalCode.ToMySqlField()},
				{Country.ToMySqlField()},
				{Telephone.ToMySqlField()},
				{email.ToMySqlField()},
				{CustomerPrePaid.ToMySqlField()},
				{PartnerType.ToMySqlField()},
				{billingdate.ToMySqlField()},
				{AllowedDaysForInvoicePayment.ToMySqlField()},
				{timezone.ToMySqlField()},
				{date1.ToMySqlField()},
				{field1.ToMySqlField()},
				{field2.ToMySqlField()},
				{field3.ToMySqlField()},
				{field4.ToMySqlField()},
				{field5.ToMySqlField()},
				{refasr.ToMySqlField()},
				{refacd.ToMySqlField()},
				{refccr.ToMySqlField()},
				{refccrbycc.ToMySqlField()},
				{refpdd.ToMySqlField()},
				{refasrfas.ToMySqlField()},
				{DefaultCurrency.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<partner,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<partner,string> whereClauseMethod)
		{
			return $@"update partner set 
				idPartner={idPartner.ToMySqlField()+" "},
				PartnerName={PartnerName.ToMySqlField()+" "},
				Address1={Address1.ToMySqlField()+" "},
				Address2={Address2.ToMySqlField()+" "},
				City={City.ToMySqlField()+" "},
				State={State.ToMySqlField()+" "},
				PostalCode={PostalCode.ToMySqlField()+" "},
				Country={Country.ToMySqlField()+" "},
				Telephone={Telephone.ToMySqlField()+" "},
				email={email.ToMySqlField()+" "},
				CustomerPrePaid={CustomerPrePaid.ToMySqlField()+" "},
				PartnerType={PartnerType.ToMySqlField()+" "},
				billingdate={billingdate.ToMySqlField()+" "},
				AllowedDaysForInvoicePayment={AllowedDaysForInvoicePayment.ToMySqlField()+" "},
				timezone={timezone.ToMySqlField()+" "},
				date1={date1.ToMySqlField()+" "},
				field1={field1.ToMySqlField()+" "},
				field2={field2.ToMySqlField()+" "},
				field3={field3.ToMySqlField()+" "},
				field4={field4.ToMySqlField()+" "},
				field5={field5.ToMySqlField()+" "},
				refasr={refasr.ToMySqlField()+" "},
				refacd={refacd.ToMySqlField()+" "},
				refccr={refccr.ToMySqlField()+" "},
				refccrbycc={refccrbycc.ToMySqlField()+" "},
				refpdd={refpdd.ToMySqlField()+" "},
				refasrfas={refasrfas.ToMySqlField()+" "},
				DefaultCurrency={DefaultCurrency.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<partner,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<partner,string> whereClauseMethod)
		{
			return $@"delete from partner 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
