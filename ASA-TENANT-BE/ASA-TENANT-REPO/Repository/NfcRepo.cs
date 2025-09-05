using ASA_TENANT_REPO.DBContext;
using ASA_TENANT_REPO.Models;
using EDUConnect_Repositories.Basic;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_REPO.Repository
{
    public class NfcRepo : GenericRepository<Nfc>
    {
        public NfcRepo(ASATENANTDBContext context) : base(context)
        {
        }

        public IQueryable<Nfc> GetFiltered(Nfc filter)
        {
            var query = _context.Nfcs.Include(nfc => nfc.Customer).AsQueryable();
            if (filter.NfcId > 0)
                query = query.Where(n => n.NfcId == filter.NfcId);
            if (filter.Status > 0)
                query = query.Where(n => n.Status == filter.Status);
            if (filter.Balance > 0)
                query = query.Where(n => n.Balance == filter.Balance);
            if (filter.CustomerId > 0)
                query = query.Where(n => n.CustomerId == filter.CustomerId);
            if (!string.IsNullOrEmpty(filter.NfcCode))
                query = query.Where(n => n.NfcCode.Contains(filter.NfcCode));
            return query.OrderBy(n => n.NfcId);
        }
    }
}
