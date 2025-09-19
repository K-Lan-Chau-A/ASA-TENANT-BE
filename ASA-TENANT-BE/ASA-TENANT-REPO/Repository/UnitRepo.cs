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
    public class UnitRepo : GenericRepository<Unit>
    {
        public UnitRepo(ASATENANTDBContext context) : base(context)
        {
        }

        public IQueryable<Unit> GetFiltered(Unit filter)
        {
            var query = _context.Units.AsQueryable();
            if (filter.UnitId > 0)
                query = query.Where(u => u.UnitId == filter.UnitId);
            if (!string.IsNullOrEmpty(filter.Name))
                query = query.Where(u => u.Name.Contains(filter.Name));
            if (filter.ShopId > 0)
                query = query.Where(u => u.ShopId == filter.ShopId);
            return query.OrderBy(u => u.UnitId);
        }
        public async Task<Unit> GetOrCreateAsync(string unitName, long shopId)
        {
            var unit = await _context.Units.FirstOrDefaultAsync(u => u.Name == unitName);
            if (unit == null)
            {
                unit = new Unit { Name = unitName , ShopId = shopId};
                _context.Units.Add(unit);
                await _context.SaveChangesAsync();
            }
            return unit;
        }
        public async Task<Unit> GetByIdAndShopIdAsync(string UnitName, long shopId)
        {
            return await _context.Units.FirstOrDefaultAsync(p => p.Name == UnitName && p.ShopId == shopId);
        }
    }
}
