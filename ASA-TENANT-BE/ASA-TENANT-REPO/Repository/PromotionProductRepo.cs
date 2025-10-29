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
    public class PromotionProductRepo : GenericRepository<PromotionProduct>
    {
        public PromotionProductRepo(ASATENANTDBContext context) : base(context)
        {
        }

        public IQueryable<PromotionProduct> GetFiltered(PromotionProduct filter)
        {
            var query = _context.PromotionProducts
                .Include(pp => pp.Product)
                .Include(pp => pp.Promotion)
                .Include(pp => pp.Unit)
                .AsQueryable();
            if (filter.PromotionProductId != 0)
                query = query.Where(x => x.PromotionProductId == filter.PromotionProductId);
            if (filter.PromotionId > 0)
                query = query.Where(x => x.PromotionId == filter.PromotionId);
            if (filter.ProductId > 0)
                query = query.Where(x => x.ProductId == filter.ProductId);
            if (filter.UnitId != 0)
                query = query.Where(x => x.UnitId == filter.UnitId);
            return query.OrderBy(x => x.PromotionProductId);
        }

        public async Task<IEnumerable<PromotionProduct>> GetByPromotionIdAsync(long promotionId)
        {
            return await _context.PromotionProducts
                .Where(pp => pp.PromotionId == promotionId)
                .ToListAsync();
        }
    }
}
