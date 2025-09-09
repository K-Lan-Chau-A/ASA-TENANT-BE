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
    public class UserRepo : GenericRepository<User>
    {
        public UserRepo(ASATENANTDBContext context) : base(context)
        {
        }

        public IQueryable<User> GetFiltered(User filter)
        {
            var query = _context.Users.AsQueryable();
            if (filter.UserId > 0)
                query = query.Where(u => u.UserId == filter.UserId);
            if (!string.IsNullOrEmpty(filter.Username))
                query = query.Where(u => u.Username.Contains(filter.Username));
            if (filter.ShopId.HasValue && filter.ShopId > 0)
                query = query.Where(u => u.ShopId == filter.ShopId);
            if (filter.Status.HasValue)
                query = query.Where(u => u.Status == filter.Status);
            if (filter.Role.HasValue)
                query = query.Where(u => u.Role == filter.Role);
            return query.OrderBy(u => u.UserId);
        }

        public async Task<User?> GetUserByUsername(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }
    }
}
