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
            if (!string.IsNullOrEmpty(filter.FullName))
                query = query.Where(u => u.FullName.Contains(filter.FullName));
            if (!string.IsNullOrEmpty(filter.PhoneNumber))
                query = query.Where(u => u.PhoneNumber.Contains(filter.PhoneNumber));
            if (!string.IsNullOrEmpty(filter.CitizenIdNumber))
                query = query.Where(u => u.CitizenIdNumber.Contains(filter.CitizenIdNumber));
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

        public async Task<User?> GetFirstUserAdmin(long id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Role == 1 && u.ShopId == id);
        }

        public async Task<List<long>> GetUserFeaturesList(long userId)
        {
            return await _context.UserFeatures
                .Where(uf => uf.UserId == userId)
                .Select(uf => uf.FeatureId)
                .Distinct()
                .ToListAsync();
        }
    }
}
