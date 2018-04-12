using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.Entity;

namespace Utils.Models
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public partial class PartnerEntities : DbContext
    {
        public PartnerEntities(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        {

        }
    }

}