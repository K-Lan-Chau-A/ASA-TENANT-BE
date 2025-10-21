using ASA_TENANT_REPO.DBContext;
using ASA_TENANT_REPO.Models;
using EDUConnect_Repositories.Basic;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASA_TENANT_REPO.Repository
{
    public class OrderRepo : GenericRepository<Order>
    {
        public OrderRepo(ASATENANTDBContext context) : base(context)
        {
        }

        public IQueryable<Order> GetFiltered(Order filter)
        {
            var query = _context.Orders.AsQueryable();

            if (filter.OrderId > 0)
                query = query.Where(o => o.OrderId == filter.OrderId);
            if (filter.CustomerId.HasValue && filter.CustomerId.Value > 0)
                query = query.Where(o => o.CustomerId == filter.CustomerId);
            if (filter.ShopId.HasValue && filter.ShopId.Value > 0)
                query = query.Where(o => o.ShopId == filter.ShopId);
            if (filter.ShiftId.HasValue && filter.ShiftId.Value > 0)
                query = query.Where(o => o.ShiftId == filter.ShiftId);
            if (filter.VoucherId.HasValue && filter.VoucherId.Value > 0)
                query = query.Where(o => o.VoucherId == filter.VoucherId);
            if (filter.Status.HasValue)
                query = query.Where(o => o.Status == filter.Status);
            if (!string.IsNullOrEmpty(filter.PaymentMethod))
                query = query.Where(o => o.PaymentMethod == filter.PaymentMethod);
            if (filter.TotalPrice.HasValue && filter.TotalPrice.Value > 0)
                query = query.Where(o => o.TotalPrice == filter.TotalPrice);
            if (filter.Discount.HasValue && filter.Discount.Value > 0)
                query = query.Where(o => o.Discount == filter.Discount);
            if (!string.IsNullOrEmpty(filter.Note))
                query = query.Where(o => o.Note.Contains(filter.Note));
            if (filter.Datetime.HasValue)
                query = query.Where(o => o.Datetime.HasValue && o.Datetime.Value.Date == filter.Datetime.Value.Date);
            if (filter.CreatedAt.HasValue)
                query = query.Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value.Date == filter.CreatedAt.Value.Date);

            return query
                .OrderByDescending(o => o.CreatedAt)
                .ThenByDescending(o => o.OrderId);
        }

        public async Task<Decimal?> GetTotalRevenueByShiftIdAsync(long shiftId)
        {
            return await _context.Orders
                .Where(o => o.ShiftId == shiftId && o.Status == 2) // 2 = Order Completed
                .SumAsync(o => (decimal?)o.TotalPrice);
        }

        public async Task<List<Order>> GetByShopIdAsync(long shopId)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                .Where(o => o.ShopId == shopId)
                .ToListAsync();
        }
    }
}
