using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class acc_chargeable:ICacheble<acc_chargeable>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{uniqueBillId.ToMySqlField()},
				{idEvent.ToMySqlField()},
				{transactionTime.ToMySqlField()},
				{assignedDirection.ToMySqlField()},
				{description.ToMySqlField()},
				{glAccountId.ToMySqlField()},
				{servicegroup.ToMySqlField()},
				{servicefamily.ToMySqlField()},
				{ProductId.ToMySqlField()},
				{idBilledUom.ToMySqlField()},
				{idQuantityUom.ToMySqlField()},
				{BilledAmount.ToMySqlField()},
				{Quantity.ToMySqlField()},
				{unitPriceOrCharge.ToMySqlField()},
				{Prefix.ToMySqlField()},
				{RateId.ToMySqlField()},
				{TaxAmount1.ToMySqlField()},
				{TaxAmount2.ToMySqlField()},
				{TaxAmount3.ToMySqlField()},
				{VatAmount1.ToMySqlField()},
				{VatAmount2.ToMySqlField()},
				{VatAmount3.ToMySqlField()},
				{OtherAmount1.ToMySqlField()},
				{OtherAmount2.ToMySqlField()},
				{OtherAmount3.ToMySqlField()},
				{OtherDecAmount1.ToMySqlField()},
				{OtherDecAmount2.ToMySqlField()},
				{OtherDecAmount3.ToMySqlField()},
				{createdByJob.ToMySqlField()},
				{changedByJob.ToMySqlField()},
				{idBillingrule.ToMySqlField()},
				{jsonDetail.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<acc_chargeable,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<acc_chargeable,string> whereClauseMethod)
		{
			return $@"update acc_chargeable set 
				id={id.ToMySqlField()+" "},
				uniqueBillId={uniqueBillId.ToMySqlField()+" "},
				idEvent={idEvent.ToMySqlField()+" "},
				transactionTime={transactionTime.ToMySqlField()+" "},
				assignedDirection={assignedDirection.ToMySqlField()+" "},
				description={description.ToMySqlField()+" "},
				glAccountId={glAccountId.ToMySqlField()+" "},
				servicegroup={servicegroup.ToMySqlField()+" "},
				servicefamily={servicefamily.ToMySqlField()+" "},
				ProductId={ProductId.ToMySqlField()+" "},
				idBilledUom={idBilledUom.ToMySqlField()+" "},
				idQuantityUom={idQuantityUom.ToMySqlField()+" "},
				BilledAmount={BilledAmount.ToMySqlField()+" "},
				Quantity={Quantity.ToMySqlField()+" "},
				unitPriceOrCharge={unitPriceOrCharge.ToMySqlField()+" "},
				Prefix={Prefix.ToMySqlField()+" "},
				RateId={RateId.ToMySqlField()+" "},
				TaxAmount1={TaxAmount1.ToMySqlField()+" "},
				TaxAmount2={TaxAmount2.ToMySqlField()+" "},
				TaxAmount3={TaxAmount3.ToMySqlField()+" "},
				VatAmount1={VatAmount1.ToMySqlField()+" "},
				VatAmount2={VatAmount2.ToMySqlField()+" "},
				VatAmount3={VatAmount3.ToMySqlField()+" "},
				OtherAmount1={OtherAmount1.ToMySqlField()+" "},
				OtherAmount2={OtherAmount2.ToMySqlField()+" "},
				OtherAmount3={OtherAmount3.ToMySqlField()+" "},
				OtherDecAmount1={OtherDecAmount1.ToMySqlField()+" "},
				OtherDecAmount2={OtherDecAmount2.ToMySqlField()+" "},
				OtherDecAmount3={OtherDecAmount3.ToMySqlField()+" "},
				createdByJob={createdByJob.ToMySqlField()+" "},
				changedByJob={changedByJob.ToMySqlField()+" "},
				idBillingrule={idBillingrule.ToMySqlField()+" "},
				jsonDetail={jsonDetail.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<acc_chargeable,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<acc_chargeable,string> whereClauseMethod)
		{
			return $@"delete from acc_chargeable 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
