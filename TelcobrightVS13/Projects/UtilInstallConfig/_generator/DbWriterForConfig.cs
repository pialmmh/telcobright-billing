using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;
using TelcobrightMediation.Config;

namespace InstallConfig._generator
{
    static class DbWriterForConfig
    {
        public static void WriteTelcobrightPartnerAndNes(PartnerEntities context, List<telcobrightpartner> partners,
            List<ne> nes)
        {
            partners.Insert(0,DummyTelcobrightPartner.getDummyTelcobrightPartner());
            nes.Insert(0, DummySwitch.getDummyNe());

            context.Database.ExecuteSqlCommand("delete from ne;");
            context.Database.ExecuteSqlCommand("delete from telcobrightpartner;");

            context.telcobrightpartners.AddRange(partners);

            context.Database.ExecuteSqlCommand("insert into telcobrightpartner values "+ string.Join(",",partners.Select(p=>p.GetExtInsertValues())));
            context.Database.ExecuteSqlCommand("update telcobrightpartner set idCustomer=0 where CustomerName = 'Dummy'");
            context.Database.ExecuteSqlCommand("insert into ne values " + string.Join(",", nes.Select(p => p.GetExtInsertValues())));
            context.Database.ExecuteSqlCommand("update ne set idSwitch=0 where SwitchName = 'dummy'");
        }
    }
}
