using EDUConnect_Repositories.Basic;
using ASA_TENANT_REPO.DBContext;
using ASA_TENANT_REPO.Models;
using Microsoft.EntityFrameworkCore;

namespace ASA_TENANT_REPO.Repository
{
    public class RankRepo : GenericRepository<Rank>
    {
        public RankRepo(ASATENANTDBContext context) : base(context)
        {
        }

        public async Task<List<Rank>> GetByShopIdAsync(long shopId)
        {
            return await _context.Set<Rank>()
                .Where(r => r.ShopId == shopId)
                .ToListAsync();
        }

        public async Task<Rank?> GetByIdAsync(int rankId)
        {
            return await _context.Set<Rank>()
                .FirstOrDefaultAsync(r => r.RankId == rankId);
        }
    }
}
