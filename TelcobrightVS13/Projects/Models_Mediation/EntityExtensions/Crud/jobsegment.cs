using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
namespace MediationModel
{
	public partial class jobsegment:ICacheble<jobsegment>
	{
		public StringBuilder GetExtInsertValues()
		{
			return new StringBuilder("(")
				.Append(this.id.ToMySqlField()).Append(",")
				.Append(this.idJob.ToMySqlField()).Append(",")
				.Append(this.segmentNumber.ToMySqlField()).Append(",")
				.Append(this.stepsCount.ToMySqlField()).Append(",")
				.Append(this.status.ToMySqlField()).Append(",")
				.Append(this.SegmentDetail.ToMySqlField()).Append(")")
				;
		}
		public  StringBuilder GetExtInsertCustom(Func<jobsegment,string> externalInsertMethod)
		{
			return new StringBuilder(externalInsertMethod.Invoke(this));
		}
		public  StringBuilder GetUpdateCommand(Func<jobsegment,string> whereClauseMethod)
		{
			return new StringBuilder("update jobsegment set ")
				.Append("id=").Append(this.id.ToMySqlField()).Append(",")
				.Append("idJob=").Append(this.idJob.ToMySqlField()).Append(",")
				.Append("segmentNumber=").Append(this.segmentNumber.ToMySqlField()).Append(",")
				.Append("stepsCount=").Append(this.stepsCount.ToMySqlField()).Append(",")
				.Append("status=").Append(this.status.ToMySqlField()).Append(",")
				.Append("SegmentDetail=").Append(this.SegmentDetail.ToMySqlField())
				.Append(whereClauseMethod.Invoke(this));
				
		}
		public  StringBuilder GetUpdateCommandCustom(Func<jobsegment,string> updateCommandMethodCustom)
		{
			return new StringBuilder(updateCommandMethodCustom.Invoke(this));
		}
		public  StringBuilder GetDeleteCommand(Func<jobsegment,string> whereClauseMethod)
		{
			return new StringBuilder($@"delete from jobsegment 
				{whereClauseMethod.Invoke(this)}");
		}
	}
}
