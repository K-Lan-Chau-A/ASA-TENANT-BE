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
            if (filter.OrderId > 0)
                query = query.Where(od => od.OrderId == filter.OrderId);
            if (filter.ProductId > 0)
                query = query.Where(od => od.ProductId == filter.ProductId);
            if (filter.ProductUnitId > 0)
                query = query.Where(od => od.ProductUnitId == filter.ProductUnitId);
            if (filter.Quantity > 0)
                query = query.Where(od => od.Quantity == filter.Quantity);
            if (filter.BasePrice > 0)
                query = query.Where(od => od.BasePrice == filter.BasePrice);
            if (filter.DiscountAmount > 0)
                query = query.Where(od => od.DiscountAmount == filter.DiscountAmount);
            if (filter.FinalPrice > 0)
                query = query.Where(od => od.FinalPrice == filter.FinalPrice);
            if (filter.Profit != 0)
                query = query.Where(od => od.Profit == filter.Profit);

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
