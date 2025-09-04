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
    public class VoucherRepo : GenericRepository<Voucher>
    {
        public VoucherRepo(ASATENANTDBContext context) : base(context)
        {
        }

        public IQueryable<Voucher> GetFiltered(Voucher filter)
        {
            var query = _context.Vouchers.AsQueryable();

            if (filter.VoucherId > 0)
                query = query.Where(v => v.VoucherId == filter.VoucherId);
            if (filter.ShopId.HasValue && filter.ShopId.Value > 0)
                query = query.Where(v => v.ShopId == filter.ShopId);
            if (filter.Type.HasValue)
                query = query.Where(v => v.Type == filter.Type);
            if (filter.Value.HasValue && filter.Value.Value > 0)
                query = query.Where(v => v.Value == filter.Value);
            if (!string.IsNullOrEmpty(filter.Code))
                query = query.Where(v => v.Code == filter.Code);
            if (filter.CreatedAt.HasValue)
                query = query.Where(v => v.CreatedAt.HasValue && v.CreatedAt.Value.Date == filter.CreatedAt.Value.Date);
            if (filter.Expired.HasValue)
                query = query.Where(v => v.Expired.HasValue && v.Expired.Value.Date == filter.Expired.Value.Date);

            return query.OrderBy(v => v.VoucherId);
        }
    }
}
