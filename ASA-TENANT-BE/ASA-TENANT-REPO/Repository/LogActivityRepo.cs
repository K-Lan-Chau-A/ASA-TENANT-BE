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
    public class LogActivityRepo : GenericRepository<LogActivity>
    {
        public LogActivityRepo(ASATENANTDBContext context) : base(context)
        {
        }

        public IQueryable<LogActivity> GetFiltered(LogActivity filter)
        {
            var query = _context.LogActivities.AsQueryable();
            if (filter.LogActivityId > 0)
                query = query.Where(la => la.LogActivityId == filter.LogActivityId);
            if (filter.UserId > 0)
                query = query.Where(la => la.UserId == filter.UserId);
            if (!string.IsNullOrEmpty(filter.Content))
                query = query.Where(la => la.Content.Contains(filter.Content));
            if (filter.Type > 0)
                query = query.Where(la => la.Type == filter.Type);
            if (filter.ShopId > 0)
                query = query.Where(la => la.ShopId == filter.ShopId);
            return query.OrderBy(la => la.LogActivityId);
        }
    }
}
