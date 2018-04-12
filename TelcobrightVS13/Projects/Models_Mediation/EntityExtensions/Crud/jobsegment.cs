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
		public string GetExtInsertValues()
		{
			return $@"(
				{id.ToMySqlField()},
				{idJob.ToMySqlField()},
				{segmentNumber.ToMySqlField()},
				{stepsCount.ToMySqlField()},
				{status.ToMySqlField()},
				{SegmentDetail.ToMySqlField()}
				)";
		}
		public  string GetExtInsertCustom(Func<jobsegment,string> externalInsertMethod)
		{
			return externalInsertMethod.Invoke(this);
		}
		public  string GetUpdateCommand(Func<jobsegment,string> whereClauseMethod)
		{
			return $@"update jobsegment set 
				id={id.ToMySqlField()+" "},
				idJob={idJob.ToMySqlField()+" "},
				segmentNumber={segmentNumber.ToMySqlField()+" "},
				stepsCount={stepsCount.ToMySqlField()+" "},
				status={status.ToMySqlField()+" "},
				SegmentDetail={SegmentDetail.ToMySqlField()+" "}
				{whereClauseMethod.Invoke(this)};
				";
		}
		public  string GetUpdateCommandCustom(Func<jobsegment,string> updateCommandMethodCustom)
		{
			return updateCommandMethodCustom.Invoke(this);
		}
		public  string GetDeleteCommand(Func<jobsegment,string> whereClauseMethod)
		{
			return $@"delete from jobsegment 
				{whereClauseMethod.Invoke(this)};
				";
		}
	}
}
