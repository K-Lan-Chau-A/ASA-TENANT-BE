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
    public class ReportRepo : GenericRepository<Report>
    {
        public ReportRepo(ASATENANTDBContext context) : base(context)
        {
        }

        public async Task GenerateDailyReportAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Nếu đã có report cho ngày hôm nay thì bỏ qua
            if (await _context.Reports.AnyAsync(r => r.Type == 1 && r.StartDate == today))
                return;

            // Lấy tất cả shift đã đóng trong ngày
            var shifts = await _context.Shifts
                .Where(s => DateOnly.FromDateTime(s.StartDate.Value) == today && s.Status == 2) // 2 = Closed
                .ToListAsync();

            if (!shifts.Any()) return;

            // Đếm số order thuộc các shift này
            var shiftIds = shifts.Select(s => s.ShiftId).ToList();
            var orderCount = await _context.Orders
                .Where(o => o.ShiftId.HasValue && shiftIds.Contains(o.ShiftId.Value))
                .CountAsync();

            var revenue = shifts.Sum(s => s.Revenue);

            var cost = await CalculateCostFIFOAsync(shiftIds);

            var grossProfit = revenue - cost;

            var report = new Report
            {
                Type = 1, // 1 = DAY
                StartDate = today,
                EndDate = today,
                CreateAt = DateTime.UtcNow,
                Revenue = revenue,
                GrossProfit = grossProfit,
                Cost = cost,
                OrderCounter = orderCount,
                ShopId = shifts.First().ShopId
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            // 🔹 Sau khi lưu Report => tạo ReportDetail
            var productSales = await _context.InventoryTransactions
                .Where(it => it.OrderId.HasValue
                          && it.Order.ShiftId.HasValue
                          && shiftIds.Contains(it.Order.ShiftId.Value)
                          && it.Type == 1) // 1 = bán hàng, 2 = nhập hàng
                .GroupBy(it => it.ProductId)
                .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity ?? 0) })
                .ToListAsync();

            foreach (var ps in productSales)
            {
                var detail = new ReportDetail
                {
                    ReportId = report.ReportId,
                    ProductId = ps.ProductId,
                    Quantity = ps.Quantity
                };
                _context.ReportDetails.Add(detail);
            }

            await _context.SaveChangesAsync();
        }

        public async Task GenerateMonthlyReportAsync()
        {
            //var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            var today = DateTime.UtcNow;
            var currentMonth = new DateOnly(today.Year, today.Month, 1); // ngày đầu tháng
            var endOfMonth = currentMonth.AddMonths(1).AddDays(-1); // ngày cuối tháng

            // Nếu đã có report cho tháng này thì bỏ qua
            if (await _context.Reports.AnyAsync(r => r.Type == 2 && r.StartDate == currentMonth))
                return;

            // Lấy tất cả shift đã đóng trong tháng
            var shifts = await _context.Shifts
                .Where(s => s.StartDate.HasValue
                    && s.Status == 2 // Closed
                    && s.StartDate.Value.Year == today.Year
                    && s.StartDate.Value.Month == today.Month)
                .ToListAsync();

            if (!shifts.Any()) return;

            // Đếm số order thuộc các shift này
            var shiftIds = shifts.Select(s => s.ShiftId).ToList();
            var orderCount = await _context.Orders
                .Where(o => o.ShiftId.HasValue && shiftIds.Contains(o.ShiftId.Value))
                .CountAsync();

            var revenue = shifts.Sum(s => s.Revenue);

            var cost = await CalculateCostFIFOAsync(shiftIds);

            var grossProfit = revenue - cost;

            var report = new Report
            {
                Type = 2, // 2 = MONTH
                StartDate = currentMonth,
                EndDate = endOfMonth,
                CreateAt = DateTime.UtcNow,
                Revenue = revenue,
                GrossProfit = grossProfit,
                Cost = cost,
                OrderCounter = orderCount,
                ShopId = shifts.First().ShopId
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            // 🔹 Tạo ReportDetail cho tháng
            var productSales = await _context.InventoryTransactions
                .Where(it => it.OrderId.HasValue
                          && it.Order.ShiftId.HasValue
                          && shiftIds.Contains(it.Order.ShiftId.Value)
                          && it.Type == 1) // 1 = bán hàng, 2 = nhập hàng
                .GroupBy(it => it.ProductId)
                .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity ?? 0) })
                .ToListAsync();

            foreach (var ps in productSales)
            {
                var detail = new ReportDetail
                {
                    ReportId = report.ReportId,
                    ProductId = ps.ProductId,
                    Quantity = ps.Quantity
                };  
                _context.ReportDetails.Add(detail);
            }

            await _context.SaveChangesAsync();
        }

        // 🔹 Hàm phụ tính cost theo FIFO
        private async Task<decimal> CalculateCostFIFOAsync(List<long> shiftIds)
        {
            decimal totalCost = 0;

            // Lấy danh sách bán theo sản phẩm trong shift
            var sales = await _context.InventoryTransactions
                .Where(it => it.Type == 1 // bán hàng
                          && it.OrderId.HasValue
                          && it.Order.ShiftId.HasValue
                          && shiftIds.Contains(it.Order.ShiftId.Value))
                .GroupBy(it => it.ProductId)
                .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity ?? 0) })
                .ToListAsync();

            foreach (var sale in sales)
            {
                int remainingToMatch = sale.Quantity;

                // Lấy danh sách nhập (Type=2) theo FIFO
                var imports = await _context.InventoryTransactions
                    .Where(it => it.Type == 2 && it.ProductId == sale.ProductId)
                    .OrderBy(it => it.CreatedAt) // FIFO = nhập trước xuất trước
                    .ToListAsync();

                foreach (var import in imports)
                {
                    if (remainingToMatch <= 0) break;

                    int available = import.Quantity ?? 0;
                    if (available <= 0) continue;

                    int used = Math.Min(remainingToMatch, available);

                    totalCost += used * (import.Price ?? 0);

                    remainingToMatch -= used;
                }
            }

            return totalCost;
        }

    }
}
