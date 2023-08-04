using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class ne:ICacheble<ne>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.idSwitch.ToMySqlField()).Append(",")
				.Append(this.idCustomer.ToMySqlField()).Append(",")
				.Append(this.idcdrformat.ToMySqlField()).Append(",")
				.Append(this.idMediationRule.ToMySqlField()).Append(",")
				.Append(this.SwitchName.ToMySqlField()).Append(",")
				.Append(this.CDRPrefix.ToMySqlField()).Append(",")
				.Append(this.FileExtension.ToMySqlField()).Append(",")
				.Append(this.Description.ToMySqlField()).Append(",")
				.Append(this.SourceFileLocations.ToMySqlField()).Append(",")
				.Append(this.BackupFileLocations.ToMySqlField()).Append(",")
				.Append(this.LoadingStopFlag.ToMySqlField()).Append(",")
				.Append(this.LoadingSpanCount.ToMySqlField()).Append(",")
				.Append(this.TransactionSizeForCDRLoading.ToMySqlField()).Append(",")
				.Append(this.DecodingSpanCount.ToMySqlField()).Append(",")
				.Append(this.SkipAutoCreateJob.ToMySqlField()).Append(",")
				.Append(this.SkipCdrListed.ToMySqlField()).Append(",")
				.Append(this.SkipCdrReceived.ToMySqlField()).Append(",")
				.Append(this.SkipCdrDecoded.ToMySqlField()).Append(",")
				.Append(this.SkipCdrBackedup.ToMySqlField()).Append(",")
				.Append(this.KeepDecodedCDR.ToMySqlField()).Append(",")
				.Append(this.KeepReceivedCdrServer.ToMySqlField()).Append(",")
				.Append(this.CcrCauseCodeField.ToMySqlField()).Append(",")
				.Append(this.SwitchTimeZoneId.ToMySqlField()).Append(",")
				.Append(this.CallConnectIndicator.ToMySqlField()).Append(",")
				.Append(this.FieldNoForTimeSummary.ToMySqlField()).Append(",")
				.Append(this.EnableSummaryGeneration.ToMySqlField()).Append(",")
				.Append(this.ExistingSummaryCacheSpanHr.ToMySqlField()).Append(",")
				.Append(this.BatchToDecodeRatio.ToMySqlField()).Append(",")
			    .Append(this.FilterDuplicateCdr.ToMySqlField()).Append(",")
                .Append(this.PrependLocationNumberToFileName.ToMySqlField()).Append(")");
		}
		public  StringBuilder GetExtInsertCustom(Func<ne,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<ne,string> whereClauseMethod)
		{
			return new StringBuilder("update ne set ")
				.Append("idSwitch=").Append(this.idSwitch.ToMySqlField()).Append(",")
				.Append("idCustomer=").Append(this.idCustomer.ToMySqlField()).Append(",")
				.Append("idcdrformat=").Append(this.idcdrformat.ToMySqlField()).Append(",")
				.Append("idMediationRule=").Append(this.idMediationRule.ToMySqlField()).Append(",")
				.Append("SwitchName=").Append(this.SwitchName.ToMySqlField()).Append(",")
				.Append("CDRPrefix=").Append(this.CDRPrefix.ToMySqlField()).Append(",")
				.Append("FileExtension=").Append(this.FileExtension.ToMySqlField()).Append(",")
				.Append("Description=").Append(this.Description.ToMySqlField()).Append(",")
				.Append("SourceFileLocations=").Append(this.SourceFileLocations.ToMySqlField()).Append(",")
				.Append("BackupFileLocations=").Append(this.BackupFileLocations.ToMySqlField()).Append(",")
				.Append("LoadingStopFlag=").Append(this.LoadingStopFlag.ToMySqlField()).Append(",")
				.Append("LoadingSpanCount=").Append(this.LoadingSpanCount.ToMySqlField()).Append(",")
				.Append("TransactionSizeForCDRLoading=").Append(this.TransactionSizeForCDRLoading.ToMySqlField()).Append(",")
				.Append("DecodingSpanCount=").Append(this.DecodingSpanCount.ToMySqlField()).Append(",")
				.Append("SkipAutoCreateJob=").Append(this.SkipAutoCreateJob.ToMySqlField()).Append(",")
				.Append("SkipCdrListed=").Append(this.SkipCdrListed.ToMySqlField()).Append(",")
				.Append("SkipCdrReceived=").Append(this.SkipCdrReceived.ToMySqlField()).Append(",")
				.Append("SkipCdrDecoded=").Append(this.SkipCdrDecoded.ToMySqlField()).Append(",")
				.Append("SkipCdrBackedup=").Append(this.SkipCdrBackedup.ToMySqlField()).Append(",")
				.Append("KeepDecodedCDR=").Append(this.KeepDecodedCDR.ToMySqlField()).Append(",")
				.Append("KeepReceivedCdrServer=").Append(this.KeepReceivedCdrServer.ToMySqlField()).Append(",")
				.Append("CcrCauseCodeField=").Append(this.CcrCauseCodeField.ToMySqlField()).Append(",")
				.Append("SwitchTimeZoneId=").Append(this.SwitchTimeZoneId.ToMySqlField()).Append(",")
				.Append("CallConnectIndicator=").Append(this.CallConnectIndicator.ToMySqlField()).Append(",")
				.Append("FieldNoForTimeSummary=").Append(this.FieldNoForTimeSummary.ToMySqlField()).Append(",")
				.Append("EnableSummaryGeneration=").Append(this.EnableSummaryGeneration.ToMySqlField()).Append(",")
				.Append("ExistingSummaryCacheSpanHr=").Append(this.ExistingSummaryCacheSpanHr.ToMySqlField()).Append(",")
				.Append("BatchToDecodeRatio=").Append(this.BatchToDecodeRatio.ToMySqlField()).Append(",")
                .Append("FilterDuplicateCdr=").Append(this.BatchToDecodeRatio.ToMySqlField()).Append(",")
                .Append("PrependLocationNumberToFileName=").Append(this.PrependLocationNumberToFileName.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<ne,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<ne,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from ne 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
