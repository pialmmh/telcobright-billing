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
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.transactionTime.ToMySqlField()).Append(",")
				.Append(this.seqId.ToMySqlField()).Append(",")
				.Append(this.debitOrCredit.ToMySqlField()).Append(",")
				.Append(this.idEvent.ToMySqlField()).Append(",")
				.Append(this.uniqueBillId.ToMySqlField()).Append(",")
				.Append(this.description.ToMySqlField()).Append(",")
				.Append(this.glAccountId.ToMySqlField()).Append(",")
				.Append(this.uomId.ToMySqlField()).Append(",")
				.Append(this.amount.ToMySqlField()).Append(",")
				.Append(this.BalanceBefore.ToMySqlField()).Append(",")
				.Append(this.BalanceAfter.ToMySqlField()).Append(",")
				.Append(this.isBillable.ToMySqlField()).Append(",")
				.Append(this.isPrepaid.ToMySqlField()).Append(",")
				.Append(this.isBilled.ToMySqlField()).Append(",")
				.Append(this.cancelled.ToMySqlField()).Append(",")
				.Append(this.createdByJob.ToMySqlField()).Append(",")
				.Append(this.changedByJob.ToMySqlField()).Append(",")
				.Append(this.jsonDetail.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<acc_temp_transaction,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<acc_temp_transaction,string> whereClauseMethod)
		{
			return new StringBuilder("update acc_temp_transaction set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("transactionTime=").Append(this.transactionTime.ToMySqlField()).Append(",")
				.Append("seqId=").Append(this.seqId.ToMySqlField()).Append(",")
				.Append("debitOrCredit=").Append(this.debitOrCredit.ToMySqlField()).Append(",")
				.Append("idEvent=").Append(this.idEvent.ToMySqlField()).Append(",")
				.Append("uniqueBillId=").Append(this.uniqueBillId.ToMySqlField()).Append(",")
				.Append("description=").Append(this.description.ToMySqlField()).Append(",")
				.Append("glAccountId=").Append(this.glAccountId.ToMySqlField()).Append(",")
				.Append("uomId=").Append(this.uomId.ToMySqlField()).Append(",")
				.Append("amount=").Append(this.amount.ToMySqlField()).Append(",")
				.Append("BalanceBefore=").Append(this.BalanceBefore.ToMySqlField()).Append(",")
				.Append("BalanceAfter=").Append(this.BalanceAfter.ToMySqlField()).Append(",")
				.Append("isBillable=").Append(this.isBillable.ToMySqlField()).Append(",")
				.Append("isPrepaid=").Append(this.isPrepaid.ToMySqlField()).Append(",")
				.Append("isBilled=").Append(this.isBilled.ToMySqlField()).Append(",")
				.Append("cancelled=").Append(this.cancelled.ToMySqlField()).Append(",")
				.Append("createdByJob=").Append(this.createdByJob.ToMySqlField()).Append(",")
				.Append("changedByJob=").Append(this.changedByJob.ToMySqlField()).Append(",")
				.Append("jsonDetail=").Append(this.jsonDetail.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<acc_temp_transaction,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<acc_temp_transaction,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from acc_temp_transaction 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
