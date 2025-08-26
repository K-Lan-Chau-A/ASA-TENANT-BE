using ASA_TENANT_REPO.DBContext;
using ASA_TENANT_REPO.Models;
using EDUConnect_Repositories.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_REPO.Repository
{
    public class InventoryTransactionRepo : GenericRepository<InventoryTransaction>
    {
        public InventoryTransactionRepo(ASATENANTDBContext context) : base(context)
        {
        }
    }
}
