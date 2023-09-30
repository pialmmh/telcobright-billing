using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Web.UI.WebControls;
using LibraryExtensions;

namespace reports
{
   
    public class NoOfCallsVsPdd
    {
        public long NoOfCalls { get; set; }
        public double Pdd { get; set; }
        public NoOfCallsVsPdd(long numberOfCalls,double pdd)
        {
            this.NoOfCalls = numberOfCalls;
            this.Pdd = pdd;
        }
    }
    public class CallStatistics
    {
        public string SwitchName { get; set; }
        public long TotalCalls { get; set; }
        public long ConnectedCalls { get; set; }
        public long ConnectedCallsbyCauseCodes { get; set; }
        public long SuccessfullCalls { get; set; }
        public double TotalActualDuration { get; set; }
        public double TotalRoundedDuration { get; set; }
        public double TotalDuration1 { get; set; }
        public double TotalDuration2 { get; set; }
        public double TotalDuration3 { get; set; }
        public double TotalDuration4 { get; set; }
        public double Asr { get; set; }
        public double Acd { get; set; }
        public double Pdd { get; set; }
        public double Ccr { get; set; }
        public double CcRbyCauseCode { get; set; }
        public double XAmount { get; set; }
        public double YAmount { get; set; }
        public double ZAmount { get; set; }
        public double BtrcRevShare { get; set; }
        public double IgwRevenue { get; set; }
        public double PartnerCost { get; set; }
        public double SupplierCost { get; set; }
        public double TotalCustomerCost { get; set; }
        public double ProfitBdt { get; set; }
        public double ProfitBdtMinute { get; set; }

        public void CalculateAveragePdd(List<NoOfCallsVsPdd> lstcallsVsPdd,int roundDigits)
        {

            long sumtotalCalls = 0;
            double totalPdd = 0;
            foreach (NoOfCallsVsPdd cp in lstcallsVsPdd)
            {
                if (cp.Pdd == 0) continue;
                sumtotalCalls += cp.NoOfCalls;
                totalPdd += cp.NoOfCalls * cp.Pdd;
            }
            if(sumtotalCalls!=0)
            this.Pdd =  Math.Round((double)totalPdd / sumtotalCalls, roundDigits);
        }
        public void CalculateAsr(int roundDigits)
        {
            if (this.TotalCalls != 0)//0 by default, no infinity case
            {
                this.Asr = Math.Round((double)100 * this.SuccessfullCalls / this.TotalCalls, roundDigits);
            }
        }
        public void CalculateAcd(int roundDigits)
        {
            if (this.SuccessfullCalls != 0)
            {
                this.Acd = Math.Round((double)this.TotalDuration1 / this.SuccessfullCalls, roundDigits);
            }
        }
        public void CalculateCcr(int roundDigits)
        {
            if(this.TotalCalls!=0)
            this.Ccr = Math.Round((double)100*this.ConnectedCalls / this.TotalCalls, roundDigits);
        }
        public void CalculateCcRbyCauseCode(int roundDigits)
        {
            if(this.TotalCalls!=0)
            this.CcRbyCauseCode = Math.Round((double)100*this.ConnectedCallsbyCauseCodes / this.TotalCalls, roundDigits);
        }
        public void CalculateProfitPerMinute(int roundDigits)
        {
            if(this.TotalDuration3!=0)
            {
                this.ProfitBdtMinute = Math.Round((double)this.ProfitBdt / this.TotalDuration3, 0);
            }
        }
    }
   
    public class TrafficReportDatasetBased
    {
        public DataSet Ds { get; set; }
        public CallStatistics CallStat { get; set; }
        public Dictionary<string,dynamic> FieldSummaries { get; set; }//key=ds fieldname, value=long/double
        public TrafficReportDatasetBased(DataSet DS)
        {
            this.Ds = DS;
            this.CallStat = new CallStatistics();
            this.CallStat = new CallStatistics();
            this.FieldSummaries = new Dictionary<string, dynamic>();
        }
        public dynamic GetDataColumnSummary(string dsColumnName)
        {
            if (this.FieldSummaries.ContainsKey(dsColumnName))
            {
                return this.FieldSummaries[dsColumnName];
            }
            return null;
        }
        public long ForceConvertToLong(object x)
        {
            if (x==DBNull.Value)
            {
                return 0;
            }
            return Convert.ToInt64(x);
        }
        public double ForceConvertToDouble(object x)
        {
            if (x == DBNull.Value)
            {
                return 0;
            }
            return Convert.ToDouble(x);
        }
    }
    
    public class DataSetWithGridView
    {
        public TrafficReportDatasetBased Tr { get; set; }
        public GridView Gv { get; set; }
        private DataTable Dt { get; set; }
        public DataSetWithGridView(TrafficReportDatasetBased Tr,GridView Gv)
        {
            this.Tr = Tr;
            this.Gv = Gv;
            this.Dt = this.Tr.Ds.Tables[0];
            PrePareDataTableForReport();
        }
        public class OrderAndSortExpression
        {
            public int ColumnOrder { get; set; }
            public string HeaderText { get; set; }
        }
        void PrePareDataTableForReport()//remove invisible columns, change field names to header text or grid and add summaryrow
        {
            //set sortexpression=datafield, no need to set that in aspx file
            
            //populate visible columns of gridview
            Dictionary<string, OrderAndSortExpression> dicVisible = new Dictionary<string, OrderAndSortExpression>();
            for (int c = 0; c < this.Gv.Columns.Count; c++)
            {
                //set sortexpression=datafield, no need to set that in aspx file
                BoundField bf = (BoundField) this.Gv.Columns[c];
                this.Gv.Columns[c].SortExpression = bf.DataField;
                if (this.Gv.Columns[c].Visible == true)
                {
                    dicVisible.Add(this.Gv.Columns[c].SortExpression.ToLower(), 
                        new OrderAndSortExpression() {ColumnOrder=c,HeaderText= this.Gv.Columns[c].HeaderText }
                        );//keep HText for use later
                }
            }
            
            
            //collection modification is a problem when deleting invisible fields of datatable
            //work around
            List<string> colDelList = new List<string>();
            DataRow summaryRow = this.Dt.NewRow();
            bool atLeastOneSummaryColumnfound = false;
            foreach (DataColumn dc in this.Dt.Columns)
            {
                //add summary value if found for this column
                dynamic summaryVal = this.Tr.GetDataColumnSummary(dc.ColumnName.ToLower());
                if (summaryVal!=null)
                {
                    atLeastOneSummaryColumnfound = true;
                    summaryRow[dc.ColumnName] = summaryVal;
                    //add number format for use in epplus
                    if( summaryVal is Int64)//determine coltype by using summary's type
                    {
                        dc.ExtendedProperties.Add("NumberFormat", "#,##0");
                    }
                    else if(summaryVal is double)
                    {
                        dc.ExtendedProperties.Add("NumberFormat", "#,##0.00");
                    }
                }
                if (dicVisible.ContainsKey(dc.ColumnName.ToLower())==false)
                {
                    colDelList.Add(dc.ColumnName);
                }
                else
                {
                    //column name is replaced with header text of grid
                    dc.ColumnName = dicVisible[dc.ColumnName.ToLower()].HeaderText;
                }
            }
            if(atLeastOneSummaryColumnfound==true)
            {
                this.Dt.Rows.Add(summaryRow);
            }
            colDelList.ForEach(c => this.Dt.Columns.Remove(c));
            //set column order
            string[] colOrder = dicVisible.Values.OrderBy(c => c.ColumnOrder).Select(c => c.HeaderText).ToArray();
            this.Dt.SetColumnsOrder(colOrder);

            
            

        }
    }
}
