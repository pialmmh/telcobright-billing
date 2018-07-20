﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;

namespace Utils
{
    public class IdVsDate
    {
        public long IdCall { get; set; }
        public DateTime StartTime { get; set; }
    }
    internal class EFTester
    {
        public void Test()
        {
            using (var dbContext=new PartnerEntities())
            {
                string sql = $@"select idcall as IdCall,starttime as StartTime from cdr limit 0,100";
                List<IdVsDate> rowIdVsDates =
                    dbContext.Database.SqlQuery<IdVsDate>(sql).ToList();
            }
        }
    }
}
