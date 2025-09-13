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
    public class UserFeatureRepo : GenericRepository<UserFeature>
    {
        public UserFeatureRepo(ASATENANTDBContext context) : base(context)
        {
        }
        public IQueryable<UserFeature> GetFiltered(UserFeature filter)
        {
            var query = _context.UserFeatures.Include(u => u.User).AsQueryable();
            if (filter.UserFeatureId > 0)
                query = query.Where(u => u.UserFeatureId == filter.UserFeatureId);
            if (filter.UserId > 0)
                query = query.Where(u => u.UserId == filter.UserId);
            if (filter.FeatureId > 0)
                query = query.Where(u => u.FeatureId == filter.FeatureId);
            if (!string.IsNullOrEmpty(filter.FeatureName))
                query = query.Where(u => u.FeatureName.Contains(filter.FeatureName));
            if (filter.IsEnabled.HasValue)
                query = query.Where(u => u.IsEnabled == filter.IsEnabled);
            if( filter.CreatedAt.HasValue)
                query = query.Where(u => u.CreatedAt <= filter.CreatedAt);
            if (filter.UpdatedAt.HasValue)
                query = query.Where(u => u.UpdatedAt <= filter.UpdatedAt);
            return query.OrderBy(u => u.UserFeatureId);
        }
        public async Task<List<UserFeature>> GetByUserIdAsync(long userId)
        {
            return await _context.UserFeatures
                .Where(uf => uf.UserId == userId)
                .ToListAsync();
        }
        public async Task AddRangeAsync(IEnumerable<UserFeature> entities)
        {
            await _context.UserFeatures.AddRangeAsync(entities);
        }

        public async Task RemoveRangeAsync(IEnumerable<UserFeature> entities)
        {
            _context.UserFeatures.RemoveRange(entities);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

    }
}
