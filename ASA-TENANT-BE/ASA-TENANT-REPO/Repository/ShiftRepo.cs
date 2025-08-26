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
    public class ShiftRepo : GenericRepository<Shift>
    {
        public ShiftRepo(ASATENANTDBContext context) : base(context)
        {
        }
    }
}
