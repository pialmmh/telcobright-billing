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
		public string GetExtInsertValues()
		{
			return $@"(
				{idSwitch.ToMySqlField()},
				{idCustomer.ToMySqlField()},
				{idcdrformat.ToMySqlField()},
				{idMediationRule.ToMySqlField()},
				{SwitchName.ToMySqlField()},
				{CDRPrefix.ToMySqlField()},
				{FileExtension.ToMySqlField()},
				{Description.ToMySqlField()},
				{SourceFileLocations.ToMySqlField()},
				{BackupFileLocations.ToMySqlField()},
				{LoadingStopFlag.ToMySqlField()},
				{LoadingSpanCount.ToMySqlField()},
				{TransactionSizeForCDRLoading.ToMySqlField()},
				{DecodingSpanCount.ToMySqlField()},
				{SkipAutoCreateJob.ToMySqlField()},
				{SkipCdrListed.ToMySqlField()},
				{SkipCdrReceived.ToMySqlField()},
				{SkipCdrDecoded.ToMySqlField()},
				{SkipCdrBackedup.ToMySqlField()},
				{KeepDecodedCDR.ToMySqlField()},
				{KeepReceivedCdrServer.ToMySqlField()},
				{CcrCauseCodeField.ToMySqlField()},
				{SwitchTimeZoneId.ToMySqlField()},
				{CallConnectIndicator.ToMySqlField()},
				{FieldNoForTimeSummary.ToMySqlField()},
				{EnableSummaryGeneration.ToMySqlField()},
				{ExistingSummaryCacheSpanHr.ToMySqlField()},
				{BatchToDecodeRatio.ToMySqlField()},
				{PrependLocationNumberToFileName.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<ne,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<ne,string> whereClauseMethod)
		{
			return $@"update ne set 
				idSwitch={idSwitch.ToMySqlField()+" "},
				idCustomer={idCustomer.ToMySqlField()+" "},
				idcdrformat={idcdrformat.ToMySqlField()+" "},
				idMediationRule={idMediationRule.ToMySqlField()+" "},
				SwitchName={SwitchName.ToMySqlField()+" "},
				CDRPrefix={CDRPrefix.ToMySqlField()+" "},
				FileExtension={FileExtension.ToMySqlField()+" "},
				Description={Description.ToMySqlField()+" "},
				SourceFileLocations={SourceFileLocations.ToMySqlField()+" "},
				BackupFileLocations={BackupFileLocations.ToMySqlField()+" "},
				LoadingStopFlag={LoadingStopFlag.ToMySqlField()+" "},
				LoadingSpanCount={LoadingSpanCount.ToMySqlField()+" "},
				TransactionSizeForCDRLoading={TransactionSizeForCDRLoading.ToMySqlField()+" "},
				DecodingSpanCount={DecodingSpanCount.ToMySqlField()+" "},
				SkipAutoCreateJob={SkipAutoCreateJob.ToMySqlField()+" "},
				SkipCdrListed={SkipCdrListed.ToMySqlField()+" "},
				SkipCdrReceived={SkipCdrReceived.ToMySqlField()+" "},
				SkipCdrDecoded={SkipCdrDecoded.ToMySqlField()+" "},
				SkipCdrBackedup={SkipCdrBackedup.ToMySqlField()+" "},
				KeepDecodedCDR={KeepDecodedCDR.ToMySqlField()+" "},
				KeepReceivedCdrServer={KeepReceivedCdrServer.ToMySqlField()+" "},
				CcrCauseCodeField={CcrCauseCodeField.ToMySqlField()+" "},
				SwitchTimeZoneId={SwitchTimeZoneId.ToMySqlField()+" "},
				CallConnectIndicator={CallConnectIndicator.ToMySqlField()+" "},
				FieldNoForTimeSummary={FieldNoForTimeSummary.ToMySqlField()+" "},
				EnableSummaryGeneration={EnableSummaryGeneration.ToMySqlField()+" "},
				ExistingSummaryCacheSpanHr={ExistingSummaryCacheSpanHr.ToMySqlField()+" "},
				BatchToDecodeRatio={BatchToDecodeRatio.ToMySqlField()+" "},
				PrependLocationNumberToFileName={PrependLocationNumberToFileName.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<ne,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<ne,string> whereClauseMethod)
		{
			return $@"delete from ne 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
