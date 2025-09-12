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
    public class ProductUnitRepo : GenericRepository<ProductUnit>
    {
        public ProductUnitRepo(ASATENANTDBContext context) : base(context)
        {
        }

        public IQueryable<ProductUnit> GetFiltered(ProductUnit filter)
        {
            var query = _context.ProductUnits
                                .Include(pu => pu.Product)
                                .Include(pu => pu.Unit)
                                .AsQueryable();

            if (filter.ProductUnitId > 0)
                query = query.Where(x => x.ProductUnitId == filter.ProductUnitId);
            if (filter.ProductId > 0)
                query = query.Where(x => x.ProductId == filter.ProductId);
            if (filter.UnitId > 0)
                query = query.Where(x => x.UnitId == filter.UnitId);
            if (filter.ShopId > 0)
                query = query.Where(x => x.ShopId == filter.ShopId);
            if (filter.ConversionFactor.HasValue && filter.ConversionFactor > 0)
                query = query.Where(x => x.ConversionFactor == filter.ConversionFactor);
            if (filter.Price.HasValue && filter.Price > 0)
                query = query.Where(x => x.Price == filter.Price);
            return query.OrderBy(x => x.ProductUnitId);
        }
        public async Task<ProductUnit> GetByProductAndUnitAsync(long productId, long unitId, long shopId)
        {
            return await _context.ProductUnits.FirstOrDefaultAsync(pu => pu.ProductId == productId && pu.UnitId == unitId && pu.ShopId == shopId);
        }
    }
}
