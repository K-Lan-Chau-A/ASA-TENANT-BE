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
    public class ShopSubscriptionRepo : GenericRepository<ShopSubscription>
    {
        public ShopSubscriptionRepo(ASATENANTDBContext context) : base(context)
        {
        }
        public IQueryable<ShopSubscription> GetFiltered(ShopSubscription filter)
        {
            var query = _context.ShopSubscriptions.AsQueryable();

            if (filter.ShopSubscriptionId > 0)
                query = query.Where(s => s.ShopSubscriptionId == filter.ShopSubscriptionId);

            if (filter.ShopId != null && filter.ShopId > 0)
                query = query.Where(s => s.ShopId == filter.ShopId);

            if (filter.PlatformProductId != null && filter.PlatformProductId > 0)
                query = query.Where(s => s.PlatformProductId == filter.PlatformProductId);

            if (filter.Status != null)
                query = query.Where(s => s.Status == filter.Status);

            if (filter.StartDate != default(DateTime))
                query = query.Where(s => s.StartDate.Date >= filter.StartDate.Date);

            if (filter.EndDate != default(DateTime))
                query = query.Where(s => s.EndDate.Date <= filter.EndDate.Date);

            return query
                .OrderByDescending(s => s.CreatedAt)
                .ThenByDescending(s => s.ShopSubscriptionId);
        }

        public async Task<ShopSubscription> GetByIdAndShopIdAsync(long id, long shopId)
        {
            return await _context.ShopSubscriptions.FirstOrDefaultAsync(s => s.ShopSubscriptionId == id && s.ShopId == shopId);
        }
    }
}
