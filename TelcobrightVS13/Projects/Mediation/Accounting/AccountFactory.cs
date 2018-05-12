using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using LibraryExtensions;
using MediationModel;
namespace TelcobrightMediation.Accounting
{
    public class AccountFactory
    {
        AccountingContext AccountingContext { get; set; }
        private static readonly object Locker = new object();
        public AccountFactory(AccountingContext accContext)
        {
            this.AccountingContext = accContext;
        }

        private account GetTemplateAccount(int idPartner, string uom,int serviceGroup,int serviceFamily,int product)
        {
            return new account()
            {
                idPartner = idPartner,
                uom = uom,
                serviceGroup = serviceGroup,
                serviceFamily = serviceFamily,
                product = product
            };
        }
        
        private account CreateAccountThroughCache(account newAcc)
        {
            lock(Locker)
            {
                CachedItem<string, account> targetAcc = null;
                targetAcc = this.AccountingContext.AccountCache.GetItemByKey(newAcc.accountName);
                if (targetAcc == null)
                {
                    newAcc.id = this.AccountingContext.AutoIncrementManager.GetNewCounter(AutoIncrementCounterType.account);
                    targetAcc = this.AccountingContext.AccountCache.InsertWithKey(newAcc, newAcc.accountName,
                        acc => acc.id > 0&&acc.idPartner>0&&acc.serviceGroup>0
                        &&acc.serviceFamily>0&&!acc.accountName.IsNullOrEmptyOrWhiteSpace());
                }
                return targetAcc.Entity;
            }
        }

        public account CreateOrGetCustomerBilled(int depth, int sg, int idPartner, int sf, int product, string uom)
        {
            //for post paid, this accont is debited with invoice generation
            //for prepaid, this acc is credited & service bank is debited
            var newAcc = GetTemplateAccount(idPartner, uom,sg,sf,product);//accountingClass=1,Asset
            newAcc.accountName = (new StringBuilder().Append("d").Append(depth)
                .Append("/sg").Append(sg).Append("/p").Append(idPartner)
                .Append("/sf").Append(sf).Append("/pd").Append(product > 0 ? product : 0)
                .Append("/custBilled").Append("/uom").Append(uom)).ToString();
            return CreateAccountThroughCache(newAcc);
        }
        public account CreateOrGetSupplierBilled(int depth, int sg, int idPartner, int sf, int product, string uom)
        {
            //for post paid, this accont is debited with invoice generation
            //for prepaid, this acc is credited & service bank is debited
            var newAcc = GetTemplateAccount(idPartner, uom, sg, sf, product);//accountingClass=1,Asset
            newAcc.accountName = (new StringBuilder().Append("d").Append(depth)
                .Append("/sg").Append(sg).Append("/p").Append(idPartner)
                .Append("/sf").Append(sf).Append("/pd").Append(product > 0 ? product : 0)
                .Append("/suppBilled").Append("/uom").Append(uom)).ToString();
            return CreateAccountThroughCache(newAcc);
        }
        public account CreateOrGetBillable(int depth, int sg, int idPartner, int sf, int product, string uom)
        {
            var newAcc = GetTemplateAccount(idPartner, uom, sg, sf, product);//accountingClass=1,Asset
            newAcc.accountName = (new StringBuilder().Append("d").Append(depth)
                .Append("/sg").Append(sg).Append("/p").Append(idPartner)
                .Append("/sf").Append(sf).Append("/pd").Append(product > 0 ? product : 0)
                .Append("/billable").Append("/uom").Append(uom)).ToString();
            newAcc.isBillable = 1;
            newAcc.isCustomerAccount = 1;
            return CreateAccountThroughCache(newAcc);
        }
        public account CreateOrGetPayable(int depth, int sg, int idPartner, int sf, int product, string uom)
        {
            var newAcc = GetTemplateAccount(idPartner, uom, sg, sf, product);//accountingClass=1,Asset
            newAcc.accountName = (new StringBuilder().Append("d").Append(depth)
                .Append("/sg").Append(sg).Append("/p").Append(idPartner)
                .Append("/sf").Append(sf).Append("/pd").Append(product > 0 ? product : 0)
                .Append("/payable").Append("/uom").Append(uom)).ToString();
            newAcc.isSupplierAccount = 1;
            return CreateAccountThroughCache(newAcc);
        }
        public account CreateOrGetCustomerAccount(int depth, int sg, int idPartner, int sf, int product, string uom)
        {
            var newAcc = GetTemplateAccount(idPartner, uom, sg, sf, product);//liability
            newAcc.accountName = (new StringBuilder().Append("d").Append(depth)
                .Append("/sg").Append(sg).Append("/p").Append(idPartner)
                .Append("/sf").Append(sf).Append("/pd").Append(product > 0 ? product : 0)
                .Append("/customer").Append("/uom").Append(uom)).ToString();
            newAcc.isCustomerAccount = 1;
            return CreateAccountThroughCache(newAcc);
        }
        public account CreateOrGetSupplierAccount(int depth, int sg, int idPartner, int sf, int product, string uom)
        {
            var newAcc = GetTemplateAccount(idPartner, uom, sg, sf, product);//liability
            newAcc.accountName = (new StringBuilder().Append("d").Append(depth)
                .Append("/sg").Append(sg).Append("/p").Append(idPartner)
                .Append("/sf").Append(sf).Append("/pd").Append(product > 0 ? product : 0)
                .Append("/supplier").Append("/uom").Append(uom)).ToString();
            newAcc.isSupplierAccount = 1;
            return CreateAccountThroughCache(newAcc);
        }
        public account CreateOrGetVirtualCash(int depth, int sg, string uom)
        {
            var newAcc = new account()
            {
                accountName = (new StringBuilder().Append("d").Append(depth)
                    .Append("/virtualcash").Append("/uom").Append(uom)).ToString()
            };
            return CreateAccountThroughCache(newAcc);
        }
        public account CreateOrGetRevenue(int depth, int sg, string uom)
        {
            var newAcc = new account()
            {
                accountName = (new StringBuilder().Append("d").Append(depth)
                    .Append("/sg").Append(sg)
                    .Append("/revenue").Append("/uom").Append(uom)).ToString()
            };
            return CreateAccountThroughCache(newAcc);
        }

        public List<KeyValuePair<string, string>> GetAccountParts(string accountName)
        {
            List<KeyValuePair<string, string>> accountParts = new List<KeyValuePair<string, string>>();
            string[] parts = accountName.Split('/');
            foreach (string part in parts)
            {
                string value = Regex.Match(part, @"\d+").Value;
                if (value != String.Empty)
                {
                    string key = part.Replace(value, string.Empty);
                    accountParts.Add(new KeyValuePair<string, string>(key, value));
                }
            }
            return accountParts;
        }
    }
}