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
    public class ShiftRepo : GenericRepository<Shift>
    {
        public ShiftRepo(ASATENANTDBContext context) : base(context)
        {
        }
        public IQueryable<Shift> GetFiltered(Shift filter)
        {
            var query = _context.Shifts.AsQueryable();

            if (filter.ShiftId > 0)
                query = query.Where(c => c.ShiftId == filter.ShiftId);
            if (filter.ShopId > 0)
                query = query.Where(c => c.ShopId == filter.ShopId);
            if (filter.UserId > 0)
                query = query.Where(c => c.UserId == filter.UserId);

            if (filter.Status.HasValue)
                query = query.Where(c => c.Status == filter.Status);
            if (filter.Revenue.HasValue)
                query = query.Where(c => c.Revenue <= filter.Revenue);
            if (filter.OpeningCash.HasValue)
                query = query.Where(c => c.OpeningCash <= filter.OpeningCash);

            if(filter.StartDate.HasValue)
                query = query.Where(c => c.StartDate >= filter.StartDate);
            if(filter.ClosedDate.HasValue)
                query = query.Where(c => c.ClosedDate <= filter.ClosedDate);

            return query.OrderByDescending(c => c.ShiftId);
        }

        public async Task<bool> HasOpenShiftAsync(long shopId)
        {
            return await _context.Shifts
                .AnyAsync(s => s.ShopId == shopId && s.Status == 1);
        }
    }
}
