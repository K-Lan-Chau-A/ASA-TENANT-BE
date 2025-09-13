using ASA_TENANT_REPO.DBContext;
using ASA_TENANT_REPO.Models;
using EDUConnect_Repositories.Basic;
using Microsoft.EntityFrameworkCore;

namespace ASA_TENANT_REPO.Repository
{
    public class ReportRepo : GenericRepository<Report>
    {
        private const int WEEKLY = 1;
        private const int MONTHLY = 2;

        public ReportRepo(ASATENANTDBContext context) : base(context) { }

        /// <summary>
        /// Tạo report hàng tuần cho từng ShopId.
        /// </summary>
        public async Task GenerateWeeklyReportAsync()
        {
            var vietnamZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamZone);

            // Tính tuần hiện tại (Mon–Sun)
            int dow = (int)localNow.DayOfWeek;
            int offsetToMonday = dow == 0 ? 6 : dow - 1;
            var startOfWeek = DateOnly.FromDateTime(localNow.Date.AddDays(-offsetToMonday));
            var endOfWeek = startOfWeek.AddDays(6);

            var startDateTime = DateTime.SpecifyKind(startOfWeek.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            var endDateTime = DateTime.SpecifyKind(endOfWeek.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);


            // Lấy tất cả shift đã đóng trong tuần
            var shifts = await _context.Shifts
                .Where(s => s.StartDate.HasValue
                    && s.StartDate.Value >= startDateTime
                    && s.StartDate.Value <= endDateTime
                    && s.Status == 2)
                .ToListAsync();

            if (!shifts.Any()) return;

            // Gom theo ShopId
            var shopGroups = shifts.GroupBy(s => s.ShopId);
            foreach (var shopGroup in shopGroups)
            {
                var shopId = shopGroup.Key;

                // Nếu đã có report weekly cho shop này tuần này -> bỏ qua
                bool exists = await _context.Reports
                    .AnyAsync(r => r.Type == WEEKLY && r.StartDate == startOfWeek && r.ShopId == shopId);
                if (exists) continue;

                var shiftIds = shopGroup.Select(s => s.ShiftId).ToList();

                var orderCount = await _context.Orders
                    .Where(o => o.ShiftId.HasValue && shiftIds.Contains(o.ShiftId.Value))
                    .CountAsync();

                var revenue = shopGroup.Sum(s => s.Revenue ?? 0m);
                var cost = await CalculateCostFIFOAsync(shiftIds);
                var grossProfit = revenue - cost;

                var report = new Report
                {
                    Type = WEEKLY,
                    StartDate = startOfWeek,
                    EndDate = endOfWeek,
                    CreateAt = DateTime.UtcNow,
                    Revenue = revenue,
                    Cost = cost,
                    GrossProfit = grossProfit,
                    OrderCounter = orderCount,
                    ShopId = shopId
                };

                _context.Reports.Add(report);
                await _context.SaveChangesAsync();

                // Chi tiết theo sản phẩm
                var productSales = await _context.InventoryTransactions
                    .Where(it => it.OrderId.HasValue
                                 && it.Order.ShiftId.HasValue
                                 && shiftIds.Contains(it.Order.ShiftId.Value)
                                 && it.Type == 1)
                    .GroupBy(it => it.ProductId)
                    .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity ?? 0) })
                    .ToListAsync();

                foreach (var ps in productSales)
                {
                    _context.ReportDetails.Add(new ReportDetail
                    {
                        ReportId = report.ReportId,
                        ProductId = ps.ProductId,
                        Quantity = ps.Quantity
                    });
                }

                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Tạo report hàng tháng cho từng ShopId, gom từ weekly report.
        /// </summary>
        public async Task GenerateMonthlyReportAsync()
        {
            // Lấy giờ hiện tại theo giờ VN
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);

            // Xác định đầu tháng hiện tại và tháng trước theo giờ VN
            var currentMonthStart = new DateOnly(localNow.Year, localNow.Month, 1);
            var prevMonthStart = currentMonthStart.AddMonths(-1);

            var monthStart = prevMonthStart;
            var monthEndExclusive = currentMonthStart; // exclusive upper bound
            var monthEndInclusive = monthEndExclusive.AddDays(-1);

            // Lấy tất cả weekly report của tháng trước
            var weeklyReports = await _context.Reports
                .Where(r => r.Type == WEEKLY
                            && r.StartDate >= monthStart
                            && r.StartDate < monthEndExclusive)
                .Include(r => r.ReportDetails)
                .ToListAsync();

            if (!weeklyReports.Any())
            {
                Console.WriteLine("⚠️ Không tìm thấy weekly report nào để tạo monthly.");
                return;
            }

            // gom theo ShopId
            var shopGroups = weeklyReports.GroupBy(r => r.ShopId);
            foreach (var shopGroup in shopGroups)
            {
                var shopId = shopGroup.Key;

                // nếu đã có monthly của tháng trước (StartDate = monthStart) thì bỏ qua
                bool exists = await _context.Reports
                    .AnyAsync(r => r.Type == MONTHLY
                                && r.StartDate == monthStart
                                && r.ShopId == shopId);

                if (exists)
                {
                    Console.WriteLine($"Monthly report đã tồn tại cho Shop {shopId} tháng {monthStart.Month}");
                    continue;
                }

                var revenue = shopGroup.Sum(r => r.Revenue ?? 0m);
                var cost = shopGroup.Sum(r => r.Cost ?? 0m);
                var grossProfit = shopGroup.Sum(r => r.GrossProfit ?? 0m);
                var orderCount = shopGroup.Sum(r => r.OrderCounter);

                var report = new Report
                {
                    Type = MONTHLY,
                    StartDate = monthStart,               // <-- monthStart (tháng trước)
                    EndDate = monthEndInclusive,         // <-- cuối tháng trước
                    CreateAt = DateTime.UtcNow,
                    Revenue = revenue,
                    Cost = cost,
                    GrossProfit = grossProfit,
                    OrderCounter = orderCount,
                    ShopId = shopId
                };

                _context.Reports.Add(report);
                await _context.SaveChangesAsync();

                var productDetails = shopGroup
                    .SelectMany(r => r.ReportDetails ?? Enumerable.Empty<ReportDetail>())
                    .GroupBy(d => d.ProductId)
                    .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                    .ToList();

                foreach (var pd in productDetails)
                {
                    _context.ReportDetails.Add(new ReportDetail
                    {
                        ReportId = report.ReportId,
                        ProductId = pd.ProductId,
                        Quantity = pd.Quantity
                    });
                }

                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Tạo Monthly report cho Shop {shopId} tháng {monthStart.Month}");
            }
        }



        // FIFO cost
        private async Task<decimal> CalculateCostFIFOAsync(List<long> shiftIds)
        {
            decimal totalCost = 0m;

            var sales = await _context.InventoryTransactions
                .Where(it => it.Type == 1
                             && it.OrderId.HasValue
                             && it.Order.ShiftId.HasValue
                             && shiftIds.Contains(it.Order.ShiftId.Value))
                .GroupBy(it => it.ProductId)
                .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity ?? 0) })
                .ToListAsync();

            if (!sales.Any()) return 0m;

            var productIds = sales.Select(s => s.ProductId).Distinct().ToList();

            var imports = await _context.InventoryTransactions
                .Where(it => it.Type == 2 && productIds.Contains(it.ProductId))
                .OrderBy(it => it.CreatedAt)
                .Select(it => new { it.ProductId, Quantity = it.Quantity ?? 0, Price = it.Price ?? 0m })
                .ToListAsync();

            var importsByProduct = imports
                .GroupBy(i => i.ProductId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => new ImportSlot { Available = x.Quantity, Price = x.Price }).ToList()
                );

            var productCosts = await _context.Products
                .Where(p => productIds.Contains(p.ProductId))
                .Select(p => new { p.ProductId, Cost = p.Cost ?? 0m })
                .ToListAsync();
            var costDict = productCosts.ToDictionary(p => p.ProductId, p => p.Cost);

            foreach (var sale in sales)
            {
                int remaining = sale.Quantity;

                if (importsByProduct.TryGetValue(sale.ProductId, out var slots))
                {
                    foreach (var slot in slots)
                    {
                        if (remaining <= 0) break;
                        if (slot.Available <= 0) continue;

                        int used = Math.Min(remaining, slot.Available);
                        totalCost += used * slot.Price;
                        slot.Available -= used;
                        remaining -= used;
                    }
                }

                if (remaining > 0)
                {
                    decimal fallback = costDict.TryGetValue(sale.ProductId.Value, out var c) ? c : 0m;
                    totalCost += remaining * fallback;
                }
            }

            return totalCost;
        }

        public IQueryable<Report> GetFiltered(Report filter)
        {
            var query = _context.Reports
                .Include(r => r.ReportDetails)
                    .ThenInclude(rd => rd.Product)
                        .ThenInclude(p => p.Category)
                .AsQueryable();

            if (filter.ReportId > 0)
                query = query.Where(r => r.ReportId == filter.ReportId);
            if (filter.Type.HasValue)
                query = query.Where(r => r.Type == filter.Type);
            if (filter.StartDate.HasValue)
                query = query.Where(r => r.StartDate >= filter.StartDate);
            if (filter.EndDate.HasValue)
                query = query.Where(r => r.EndDate <= filter.EndDate);
            if (filter.ShopId > 0)
                query = query.Where(r => r.ShopId == filter.ShopId);
            if (filter.Revenue > 0)
                query = query.Where(r => r.Revenue >= filter.Revenue);
            if (filter.Cost > 0)
                query = query.Where(r => r.Cost >= filter.Cost);
            if (filter.GrossProfit > 0)
                query = query.Where(r => r.GrossProfit >= filter.GrossProfit);
            if (filter.OrderCounter > 0)
                query = query.Where(r => r.OrderCounter >= filter.OrderCounter);
            if (filter.CreateAt.HasValue && filter.CreateAt > DateTime.MinValue)
                query = query.Where(r => r.CreateAt >= filter.CreateAt);

            return query.OrderBy(r => r.ReportId);
        }

        private class ImportSlot
        {
            public int Available { get; set; }
            public decimal Price { get; set; }
        }
    }
}
