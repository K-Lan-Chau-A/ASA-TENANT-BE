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
    public class NotificationRepo : GenericRepository<Notification>
    {
        public NotificationRepo(ASATENANTDBContext context) : base(context)
        {
        }
        public IQueryable<Notification> GetFiltered(Notification filter)
        {
            var query = _context.Notifications.AsQueryable();

            if (filter.NotificationId > 0)
                query = query.Where(c => c.NotificationId == filter.NotificationId);
            if (!string.IsNullOrEmpty(filter.Title))
                query = query.Where(c => c.Title.Contains(filter.Title));

            if (filter.ShopId > 0)
                query = query.Where(c => c.ShopId == filter.ShopId);
            if(filter.UserId > 0)
                query = query.Where(c => c.UserId == filter.UserId);
            if(!string.IsNullOrEmpty(filter.Content))
                query = query.Where(c => c.Content.Contains(filter.Content));
            if(filter.Type.HasValue)
                query = query.Where(c => c.Type == filter.Type);
            if(filter.IsRead.HasValue)
                query = query.Where(c => c.IsRead == filter.IsRead);
            if(filter.CreatedAt.HasValue)
                query = query.Where(c => c.CreatedAt <= filter.CreatedAt);

            return query.OrderByDescending(c => c.NotificationId);
        }
    }
}
