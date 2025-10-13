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
    public class RankRepo : GenericRepository<Rank>
    {
        public RankRepo(ASATENANTDBContext context) : base(context)
        {
        }
        public IQueryable<Rank> GetFiltered(Rank filter)
        {
            var query = _context.Ranks.AsQueryable();
            if (filter.RankId > 0)
                query = query.Where(x => x.RankId == filter.RankId);
            if (filter.RankName != null)
                query = query.Where(x => x.RankName == filter.RankName);
            if (filter.Benefit != null)
                query = query.Where(x => x.Benefit <= filter.Benefit);
            if (filter.Threshold != null)
                query = query.Where(x => x.Threshold == filter.Threshold);
            if (filter.ShopId > 0)
                query = query.Where(x => x.ShopId == filter.ShopId);
            return query.OrderBy(x => x.RankId);
        }

        // Validation helper methods
        public async Task<bool> IsRankNameExistsAsync(string rankName, long shopId, int? excludeRankId = null)
        {
            var query = _context.Ranks.Where(x => x.RankName.ToLower() == rankName.ToLower() && x.ShopId == shopId);
            if (excludeRankId.HasValue)
                query = query.Where(x => x.RankId != excludeRankId.Value);
            return await query.AnyAsync();
        }

        public async Task<bool> IsBenefitExistsAsync(double benefit, long shopId, int? excludeRankId = null)
        {
            var query = _context.Ranks.Where(x => x.Benefit == benefit && x.ShopId == shopId);
            if (excludeRankId.HasValue)
                query = query.Where(x => x.RankId != excludeRankId.Value);
            return await query.AnyAsync();
        }

        public async Task<bool> IsThresholdExistsAsync(double threshold, long shopId, int? excludeRankId = null)
        {
            var query = _context.Ranks.Where(x => x.Threshold == threshold && x.ShopId == shopId);
            if (excludeRankId.HasValue)
                query = query.Where(x => x.RankId != excludeRankId.Value);
            return await query.AnyAsync();
        }

        public async Task<List<Rank>> GetRanksByShopIdAsync(long shopId, int? excludeRankId = null)
        {
            var query = _context.Ranks.Where(x => x.ShopId == shopId);
            if (excludeRankId.HasValue)
                query = query.Where(x => x.RankId != excludeRankId.Value);
            return await query.OrderBy(x => x.Threshold).ToListAsync();
        }

        // Helper methods for null threshold logic
        public async Task<Rank> GetRankWithNullThresholdAsync(long shopId, int? excludeRankId = null)
        {
            var query = _context.Ranks.Where(x => x.ShopId == shopId && x.Threshold == null);
            if (excludeRankId.HasValue)
                query = query.Where(x => x.RankId != excludeRankId.Value);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<double?> GetMaxBenefitInShopAsync(long shopId, int? excludeRankId = null)
        {
            var query = _context.Ranks.Where(x => x.ShopId == shopId && x.Benefit.HasValue);
            if (excludeRankId.HasValue)
                query = query.Where(x => x.RankId != excludeRankId.Value);
            return await query.MaxAsync(x => x.Benefit);
        }

        public async Task<bool> HasRankWithNullThresholdAsync(long shopId, int? excludeRankId = null)
        {
            var query = _context.Ranks.Where(x => x.ShopId == shopId && x.Threshold == null);
            if (excludeRankId.HasValue)
                query = query.Where(x => x.RankId != excludeRankId.Value);
            return await query.AnyAsync();
        }
    }
}
