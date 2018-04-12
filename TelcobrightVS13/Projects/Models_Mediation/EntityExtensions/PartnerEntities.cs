using System.Data.Common;
using System.Data.Entity;

namespace MediationModel
{
    [DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
    public partial class PartnerEntities : DbContext
    {
        //by new connection string
        public PartnerEntities(string newConStr)
            : base(newConStr)
        {
        }

        //use existing connection
        public PartnerEntities(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        {
            
        }
    }
}