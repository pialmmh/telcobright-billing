using System;
using System.Collections.Generic;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text;
using MediationModel;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.MefData.GenericAssignment;
using Newtonsoft.Json;
using TelcobrightMediation.Config;

namespace PartnerRules
{
    [Export("GenericAssignmentFactory", typeof(IGenericParameterAssignmentFactory))]
    public class DbRowParameterAssignmentFactory : IGenericParameterAssignmentFactory
    {
        public string FactoryName => GetType().Name;
        public void GetParametersAsTypedInstance(Dictionary<string, string> parameters,
            Action<Dictionary<string, string>> methodToCreateParameterObjectContainer )
        {
            methodToCreateParameterObjectContainer.Invoke(parameters);
        }

        public void WriteAssignmentToDb(genericparameterassignment genericparameterassignment, PartnerEntities context)
        {
            Dictionary<string, string> parameters=JsonConvert.DeserializeObject<Dictionary<string,string>>
                (genericparameterassignment.JsonExpAssignedTo);
            DbCommand cmd = ConnectionManager.CreateCommandFromDbContext(context);
            if (cmd.Connection.State != ConnectionState.Open)
            {
                cmd.Connection.Open();
            }
            cmd.CommandText= new StringBuilder(StaticExtInsertColumnHeaders.genericparameterassignment)
                .Append(genericparameterassignment.GetExtInsertValues()).ToString();
            cmd.ExecuteNonQuery();
        }
    }
}
