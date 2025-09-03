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
    public class PromotionRepo : GenericRepository<Promotion>
    {
        public PromotionRepo(ASATENANTDBContext context) : base(context)
        {
        }
         
        public IQueryable<Promotion> GetFiltered(Promotion filter)
        {
            var query = _context.Promotions.AsQueryable();
            if (filter.PromotionId > 0)
                query = query.Where(x => x.PromotionId == filter.PromotionId);
            if (filter.StartDate != null)
                query = query.Where(x => x.StartDate == filter.StartDate);
            if (filter.EndDate != null)
                query = query.Where(x => x.EndDate == filter.EndDate);
            if (filter.StartTime != null)
                query = query.Where(x => x.StartTime == filter.StartTime);
            if (filter.EndTime != null)
                query = query.Where(x => x.EndTime == filter.EndTime);
            if (filter.Value != null)
                query = query.Where(x => x.Value == filter.Value);
            if (filter.Type != null)
                query = query.Where(x => x.Type == filter.Type);
            if (filter.Status != null)
                query = query.Where(x => x.Status == filter.Status);
            if (!string.IsNullOrEmpty(filter.Name))
                query = query.Where(x => x.Name.Contains(filter.Name));
            if (filter.ShopId > 0)
                query = query.Where(x => x.ShopId == filter.ShopId);
            return query.OrderBy(x => x.PromotionId);
        }
    }
}
