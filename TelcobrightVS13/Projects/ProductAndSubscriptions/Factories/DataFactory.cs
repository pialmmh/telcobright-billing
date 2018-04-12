using System.ComponentModel.Composition;
using System;
using System.Collections.Generic;
using MediationModel;

namespace TelcobrightMediation.ProductAndContract
{
    [Export("ProductFactory", typeof(IProductFactory))]
    public class DataFactory : IDataFactory
    {
        public string FactoryName { get { return "dataFactory"; } }
        public IProduct CreateDataVolume(uom uoM,
                                        string pricingRule,
                                        IEnumerable<IProductSpecification> spec)
        {
            return new DataVolume();
        }
        public IProduct CreateBandwidth()
        {
            throw new NotImplementedException();
        }
        public IProduct CreateServiceCredit()
        {
            throw new NotImplementedException();
        }
        public IProduct CreateServiceCredit<T>()
        {
            throw new NotImplementedException();
        }

        public IProduct CreateDataVolume()
        {
            throw new NotImplementedException();
        }
    }
}
