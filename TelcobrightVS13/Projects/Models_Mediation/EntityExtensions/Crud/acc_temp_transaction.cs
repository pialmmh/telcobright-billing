using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class acc_temp_transaction:ICacheble<acc_temp_transaction>
	{
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{transactionTime.ToMySqlField()},
				{seqId.ToMySqlField()},
				{debitOrCredit.ToMySqlField()},
				{idEvent.ToMySqlField()},
				{uniqueBillId.ToMySqlField()},
				{description.ToMySqlField()},
				{glAccountId.ToMySqlField()},
				{uomId.ToMySqlField()},
				{amount.ToMySqlField()},
				{BalanceBefore.ToMySqlField()},
				{BalanceAfter.ToMySqlField()},
				{isBillable.ToMySqlField()},
				{isPrepaid.ToMySqlField()},
				{isBilled.ToMySqlField()},
				{cancelled.ToMySqlField()},
				{createdByJob.ToMySqlField()},
				{changedByJob.ToMySqlField()},
				{jsonDetail.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<acc_temp_transaction,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<acc_temp_transaction,string> whereClauseMethod)
		{
			return $@"update acc_temp_transaction set 
				id={id.ToMySqlField()+" "},
				transactionTime={transactionTime.ToMySqlField()+" "},
				seqId={seqId.ToMySqlField()+" "},
				debitOrCredit={debitOrCredit.ToMySqlField()+" "},
				idEvent={idEvent.ToMySqlField()+" "},
				uniqueBillId={uniqueBillId.ToMySqlField()+" "},
				description={description.ToMySqlField()+" "},
				glAccountId={glAccountId.ToMySqlField()+" "},
				uomId={uomId.ToMySqlField()+" "},
				amount={amount.ToMySqlField()+" "},
				BalanceBefore={BalanceBefore.ToMySqlField()+" "},
				BalanceAfter={BalanceAfter.ToMySqlField()+" "},
				isBillable={isBillable.ToMySqlField()+" "},
				isPrepaid={isPrepaid.ToMySqlField()+" "},
				isBilled={isBilled.ToMySqlField()+" "},
				cancelled={cancelled.ToMySqlField()+" "},
				createdByJob={createdByJob.ToMySqlField()+" "},
				changedByJob={changedByJob.ToMySqlField()+" "},
				jsonDetail={jsonDetail.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<acc_temp_transaction,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<acc_temp_transaction,string> whereClauseMethod)
		{
			return $@"delete from acc_temp_transaction 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
