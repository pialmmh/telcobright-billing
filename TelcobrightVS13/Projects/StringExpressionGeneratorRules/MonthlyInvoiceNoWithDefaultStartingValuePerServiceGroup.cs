using System;
using System.ComponentModel.Composition;
using MediationModel;
using TelcobrightMediation;
using System.Collections.Generic;
using System.Data.Common;
//using MySql.Data;
using MySql.Data.MySqlClient;
using TelcobrightInfra;
namespace StringExpressionGeneratorRules
{
    [Export(typeof(IStringExpressionGenerator))]
    public class MonthlyInvoiceNoWithDefaultStartingValuePerServiceGroup : IStringExpressionGenerator
    {
        public string RuleName { get; }
        public string HelpText { get; }
        public int Id { get; }
        public object Data { get; set; }
        private long DefaultStartingValue { get; set; }
        public bool IsPrepared { get; private set; }
        public void Prepare()
        {
            this.DefaultStartingValue = (long) this.Data;
            this.IsPrepared = true;
        }
        public string GetStringExpression(Dictionary<string,object> input)
        {
            if(this.IsPrepared==false)
                throw new Exception("Rule not prepared, rule name:" + this.RuleName);

            invoice invoice = (invoice) input["invoice"];
            string serviceGroupName = (string) input["serviceGroupName"];
            DbCommand cmd = (DbCommand)input["dbcommand"];

            if (invoice.INVOICE_DATE == null)
            {
                throw new Exception("Invoice date found null while generating reference no expresion. ");
            }

            var invoiceDate = Convert.ToDateTime(invoice.INVOICE_DATE);

            //TupleIncrementManager ti= new TupleIncrementManager(this.DefaultStartingValue);
            string tupleExpression = invoiceDate.Year.ToString() + "-" + invoiceDate.Month.ToString() + "/" + serviceGroupName;
            //var x = ti.getIncrementalValue(tupleExpression,cmd)

            return tupleExpression; //e.g. 2023-12/domestic
        }
    }
}
