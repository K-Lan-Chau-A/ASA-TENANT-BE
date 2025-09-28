using ASA_TENANT_REPO.DBContext;
using ASA_TENANT_REPO.Models;
using ASA_TENANT_REPO.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

public class ReportRepoTests
{
    // ==================== TEST CASES FOR REPORT GENERATION ====================

    /*
     * 1) GenerateWeeklyReportAsync_ShouldCreateWeeklyReportFromShifts
     *    - Kiểm tra tạo báo cáo tuần từ các shift (Status = 2).
     *    - Bao gồm Revenue, Cost, GrossProfit, OrderCounter, ReportDetails.
     *
     * 2) GenerateMonthlyReportAsync_ShouldAggregateWeeklyReports
     *    - Kiểm tra tạo báo cáo tháng từ các weekly report (tháng trước).
     *    - Kiểm tra cộng dồn Revenue, Cost, GrossProfit, OrderCounter, ReportDetails.
     *
     * 3) GenerateMonthlyReportAsync_ShouldAggregate4WeeklyReports
     *    - Kiểm tra gộp nhiều weekly report (4 tuần) trong tháng.
     *    - Đảm bảo số liệu cộng dồn chính xác.
     *
     * 4) GenerateMonthlyReportAsync_ShouldOnlyAggregateSameShopId
     *    - Kiểm tra monthly report chỉ gộp các weekly report cùng ShopId.
     *    - Đảm bảo dữ liệu shop khác không bị lẫn.
     *
     * 5) GenerateMonthlyReportAsync_ShouldHandleMonthWith5WeeklyReports
     *    - Kiểm tra tháng có 5 weekly report (hoặc số lượng khác nhau giữa các shop).
     *    - Đảm bảo tổng hợp chính xác cho từng shop riêng biệt.
     */


    private ASATENANTDBContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ASATENANTDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ASATENANTDBContext(options);
    }

    [Fact]
    public async Task GenerateMonthlyReportAsync_ShouldAggregateWeeklyReports()
    {
        var context = GetInMemoryDbContext();
        var repo = new ReportRepo(context);
        int shopId = 1;

        // Giả lập weekly reports của tháng trước (8/2025)
        context.Reports.AddRange(
            new Report
            {
                Type = 1,
                ShopId = shopId,
                StartDate = new DateOnly(2025, 8, 1),
                EndDate = new DateOnly(2025, 8, 7),
                Revenue = 1000,
                Cost = 400,
                GrossProfit = 600,
                OrderCounter = 10,
                ReportDetails =
                {
                    new ReportDetail { ProductId = 101, Quantity = 5 },
                    new ReportDetail { ProductId = 102, Quantity = 5 }
                }
            },
            new Report
            {
                Type = 1,
                ShopId = shopId,
                StartDate = new DateOnly(2025, 8, 8),
                EndDate = new DateOnly(2025, 8, 14),
                Revenue = 2000,
                Cost = 1000,
                GrossProfit = 1000,
                OrderCounter = 20,
                ReportDetails =
                {
                    new ReportDetail { ProductId = 101, Quantity = 20 }
                }
            }
        );
        await context.SaveChangesAsync();

        // Act
        await repo.GenerateMonthlyReportAsync();

        // Assert
        var monthly = await context.Reports
            .Include(r => r.ReportDetails)
            .FirstOrDefaultAsync(r => r.Type == 2 && r.ShopId == shopId);

        Assert.NotNull(monthly);
        Assert.Equal(3000, monthly.Revenue);
        Assert.Equal(1400, monthly.Cost);
        Assert.Equal(1600, monthly.GrossProfit);
        Assert.Equal(30, monthly.OrderCounter);
        Assert.Equal(25, monthly.ReportDetails.First(d => d.ProductId == 101).Quantity);
        Assert.Equal(5, monthly.ReportDetails.First(d => d.ProductId == 102).Quantity);
    }

    [Fact]
    public async Task GenerateMonthlyReportAsync_ShouldAggregate4WeeklyReports()
    {
        var context = GetInMemoryDbContext();
        var repo = new ReportRepo(context);
        int shopId = 1;

        for (int i = 0; i < 4; i++)
        {
            context.Reports.Add(new Report
            {
                Type = 1,
                ShopId = shopId,
                StartDate = new DateOnly(2025, 8, 1).AddDays(i * 7),
                EndDate = new DateOnly(2025, 8, 7).AddDays(i * 7),
                Revenue = 1000 * (i + 1),
                Cost = 500 * (i + 1),
                GrossProfit = 500 * (i + 1),
                OrderCounter = 10 * (i + 1),
                ReportDetails =
                {
                    new ReportDetail { ProductId = 101, Quantity = 5 * (i + 1) }
                }
            });
        }
        await context.SaveChangesAsync();

        await repo.GenerateMonthlyReportAsync();

        var monthly = await context.Reports
            .Include(r => r.ReportDetails)
            .FirstOrDefaultAsync(r => r.Type == 2 && r.ShopId == shopId);

        Assert.NotNull(monthly);
        Assert.Equal(10000, monthly.Revenue);
        Assert.Equal(5000, monthly.Cost);
        Assert.Equal(5000, monthly.GrossProfit);
        Assert.Equal(100, monthly.OrderCounter);
        Assert.Equal(50, monthly.ReportDetails.First(d => d.ProductId == 101).Quantity);
    }

    [Fact]
    public async Task GenerateMonthlyReportAsync_ShouldOnlyAggregateSameShopId()
    {
        var context = GetInMemoryDbContext();
        var repo = new ReportRepo(context);

        context.Reports.AddRange(
            new Report
            {
                Type = 1,
                ShopId = 1,
                StartDate = new DateOnly(2025, 8, 1),
                EndDate = new DateOnly(2025, 8, 7),
                Revenue = 1000,
                Cost = 400,
                GrossProfit = 600,
                OrderCounter = 10,
                ReportDetails = { new ReportDetail { ProductId = 101, Quantity = 5 } }
            },
            new Report
            {
                Type = 1,
                ShopId = 2,
                StartDate = new DateOnly(2025, 8, 1),
                EndDate = new DateOnly(2025, 8, 7),
                Revenue = 2000,
                Cost = 1000,
                GrossProfit = 1000,
                OrderCounter = 20,
                ReportDetails = { new ReportDetail { ProductId = 102, Quantity = 10 } }
            }
        );
        await context.SaveChangesAsync();

        await repo.GenerateMonthlyReportAsync();

        var monthlyShop1 = await context.Reports.Include(r => r.ReportDetails).FirstOrDefaultAsync(r => r.Type == 2 && r.ShopId == 1);
        var monthlyShop2 = await context.Reports.Include(r => r.ReportDetails).FirstOrDefaultAsync(r => r.Type == 2 && r.ShopId == 2);

        Assert.NotNull(monthlyShop1);
        Assert.NotNull(monthlyShop2);
        Assert.Equal(1000, monthlyShop1.Revenue);
        Assert.Equal(2000, monthlyShop2.Revenue);
        Assert.Single(monthlyShop1.ReportDetails);
        Assert.Single(monthlyShop2.ReportDetails);
        Assert.Equal(101, monthlyShop1.ReportDetails.First().ProductId);
        Assert.Equal(102, monthlyShop2.ReportDetails.First().ProductId);
    }

    [Fact]
    public async Task GenerateMonthlyReportAsync_ShouldHandleMonthWith5WeeklyReports()
    {
        var context = GetInMemoryDbContext();
        var repo = new ReportRepo(context);
        int shop1 = 1, shop2 = 2;
        var monthStart = new DateOnly(2025, 8, 1);

        for (int i = 0; i < 5; i++)
        {
            context.Reports.Add(new Report
            {
                Type = 1,
                ShopId = shop1,
                StartDate = monthStart.AddDays(i * 7),
                EndDate = monthStart.AddDays(i * 7 + 6),
                Revenue = 1000,
                Cost = 400,
                GrossProfit = 600,
                OrderCounter = 10,
                ReportDetails = { new ReportDetail { ProductId = 101, Quantity = 5 } }
            });
        }

        for (int i = 0; i < 3; i++)
        {
            context.Reports.Add(new Report
            {
                Type = 1,
                ShopId = shop2,
                StartDate = monthStart.AddDays(i * 7),
                EndDate = monthStart.AddDays(i * 7 + 6),
                Revenue = 2000,
                Cost = 800,
                GrossProfit = 1200,
                OrderCounter = 20,
                ReportDetails = { new ReportDetail { ProductId = 102, Quantity = 10 } }
            });
        }
        await context.SaveChangesAsync();

        await repo.GenerateMonthlyReportAsync();

        var monthlyShop1 = await context.Reports.Include(r => r.ReportDetails).FirstOrDefaultAsync(r => r.Type == 2 && r.ShopId == shop1);
        var monthlyShop2 = await context.Reports.Include(r => r.ReportDetails).FirstOrDefaultAsync(r => r.Type == 2 && r.ShopId == shop2);

        Assert.NotNull(monthlyShop1);
        Assert.Equal(5000, monthlyShop1.Revenue);
        Assert.Equal(2000, monthlyShop1.Cost);
        Assert.Equal(3000, monthlyShop1.GrossProfit);
        Assert.Equal(50, monthlyShop1.OrderCounter);
        Assert.Equal(25, monthlyShop1.ReportDetails.First().Quantity);

        Assert.NotNull(monthlyShop2);
        Assert.Equal(6000, monthlyShop2.Revenue);
        Assert.Equal(2400, monthlyShop2.Cost);
        Assert.Equal(3600, monthlyShop2.GrossProfit);
        Assert.Equal(60, monthlyShop2.OrderCounter);
        Assert.Equal(30, monthlyShop2.ReportDetails.First().Quantity);
    }


}
