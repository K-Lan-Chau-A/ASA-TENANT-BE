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
    public class OrderDetailRepo : GenericRepository<OrderDetail>
    {
        public OrderDetailRepo(ASATENANTDBContext context) : base(context)
        {
        }

        public IQueryable<OrderDetail> GetFiltered(OrderDetail filter)
        {
            var query = _context.OrderDetails.AsQueryable();

            if (filter.OrderDetailId > 0)
                query = query.Where(od => od.OrderDetailId == filter.OrderDetailId);
            if (filter.OrderId.HasValue && filter.OrderId.Value > 0)
                query = query.Where(od => od.OrderId == filter.OrderId);
            if (filter.ProductId.HasValue && filter.ProductId.Value > 0)
                query = query.Where(od => od.ProductId == filter.ProductId);
            if (filter.ProductUnitId.HasValue && filter.ProductUnitId.Value > 0)
                query = query.Where(od => od.ProductUnitId == filter.ProductUnitId);
            if (filter.Quantity.HasValue && filter.Quantity.Value > 0)
                query = query.Where(od => od.Quantity == filter.Quantity);
            if (filter.TotalPrice.HasValue && filter.TotalPrice.Value > 0)
                query = query.Where(od => od.TotalPrice == filter.TotalPrice);

            return query.OrderBy(od => od.OrderDetailId);
        }

        public async Task<List<OrderDetail>> GetByShopIdAsync(long shopId)
        {
            return await _context.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Product)
                .Where(od => od.Order.ShopId == shopId)
                .ToListAsync();
        }
    }
}
