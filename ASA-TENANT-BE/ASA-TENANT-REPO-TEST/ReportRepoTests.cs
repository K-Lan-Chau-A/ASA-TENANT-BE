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
public async Task GenerateWeeklyReportAsync_ShouldCreateWeeklyReportFromShifts()
{
    // Arrange
    var context = GetInMemoryDbContext();
    var repo = new ReportRepo(context);

    int shopId = 1;
    var baseDate = new DateTime(2025, 9, 10); // ví dụ tuần 8-14/9

    // Tạo 2 shift trong tuần
    context.Shifts.AddRange(
        new Shift { ShiftId = 1, ShopId = shopId, StartDate = baseDate, Status = 2, Revenue = 1000 },
        new Shift { ShiftId = 2, ShopId = shopId, StartDate = baseDate.AddDays(2), Status = 2, Revenue = 2000 }
    );

    // Tạo inventory transaction cho các shift
    context.InventoryTransactions.AddRange(
        new InventoryTransaction { OrderId = 1, ProductId = 101, Type = 1, Quantity = 5, Price = 50 },
        new InventoryTransaction { OrderId = 2, ProductId = 102, Type = 1, Quantity = 10, Price = 30 }
    );

    await context.SaveChangesAsync();

    // Thêm Orders liên kết với Shift
    context.Orders.AddRange(
            new Order { OrderId = 1, ShiftId = 1 },
            new Order { OrderId = 2, ShiftId = 2 }
     );
    await context.SaveChangesAsync();

    // Act
    await repo.GenerateWeeklyReportAsync();

    // Assert
    var weekly = await context.Reports
        .Include(r => r.ReportDetails)
        .FirstOrDefaultAsync(r => r.Type == 1 && r.ShopId == shopId);

    Assert.NotNull(weekly);
    Assert.Equal(3000, weekly.Revenue);
    Assert.Equal(0, weekly.Cost); // nếu dùng cost FIFO sẽ tính theo logic repo
    Assert.Equal(3000, weekly.GrossProfit);
    Assert.Equal(2, weekly.OrderCounter);

    var detail101 = weekly.ReportDetails.FirstOrDefault(d => d.ProductId == 101);
    Assert.NotNull(detail101);
    Assert.Equal(5, detail101.Quantity);

    var detail102 = weekly.ReportDetails.FirstOrDefault(d => d.ProductId == 102);
    Assert.NotNull(detail102);
    Assert.Equal(10, detail102.Quantity);
}


    [Fact]
    public async Task GenerateMonthlyReportAsync_ShouldAggregateWeeklyReports()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var repo = new ReportRepo(context);

        int shopId = 1;

        // Phải tạo weekly ở tháng trước (tháng 8), vì hàm gom tháng 8
        var weekly1 = new Report
        {
            ReportId = 1,
            Type = 1, // WEEKLY
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
        };

        var weekly2 = new Report
        {
            ReportId = 2,
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
        };

        context.Reports.AddRange(weekly1, weekly2);
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

        // Gộp detail
        var detail101 = monthly.ReportDetails.FirstOrDefault(d => d.ProductId == 101);
        Assert.NotNull(detail101);
        Assert.Equal(25, detail101.Quantity);

        var detail102 = monthly.ReportDetails.FirstOrDefault(d => d.ProductId == 102);
        Assert.NotNull(detail102);
        Assert.Equal(5, detail102.Quantity);
    }


    [Fact]
    public async Task GenerateMonthlyReportAsync_ShouldAggregate4WeeklyReports()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var repo = new ReportRepo(context);

        int shopId = 1;

        // Tạo 4 weekly report trong tháng 8/2025
        for (int i = 0; i < 4; i++)
        {
            context.Reports.Add(new Report
            {
                Type = 1, // WEEKLY
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

        // Act
        await repo.GenerateMonthlyReportAsync();

        // Assert
        var monthly = await context.Reports
            .Include(r => r.ReportDetails)
            .FirstOrDefaultAsync(r => r.Type == 2 && r.ShopId == shopId);

        Assert.NotNull(monthly);
        Assert.Equal(10000, monthly.Revenue);
        Assert.Equal(5000, monthly.Cost);
        Assert.Equal(5000, monthly.GrossProfit);
        Assert.Equal(100, monthly.OrderCounter);

        var detail101 = monthly.ReportDetails.FirstOrDefault(d => d.ProductId == 101);
        Assert.NotNull(detail101);
        Assert.Equal(50, detail101.Quantity);
    }


    [Fact]
    public async Task GenerateMonthlyReportAsync_ShouldOnlyAggregateSameShopId()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var repo = new ReportRepo(context);

        // Shop 1
        context.Reports.Add(new Report
        {
            Type = 1,
            ShopId = 1,
            StartDate = new DateOnly(2025, 8, 1),
            EndDate = new DateOnly(2025, 8, 7),
            Revenue = 1000,
            Cost = 400,
            GrossProfit = 600,
            OrderCounter = 10,
            ReportDetails =
        {
            new ReportDetail { ProductId = 101, Quantity = 5 }
        }
        });

        // Shop 2
        context.Reports.Add(new Report
        {
            Type = 1,
            ShopId = 2,
            StartDate = new DateOnly(2025, 8, 1),
            EndDate = new DateOnly(2025, 8, 7),
            Revenue = 2000,
            Cost = 1000,
            GrossProfit = 1000,
            OrderCounter = 20,
            ReportDetails =
        {
            new ReportDetail { ProductId = 102, Quantity = 10 }
        }
        });

        await context.SaveChangesAsync();

        // Act
        await repo.GenerateMonthlyReportAsync();

        // Assert
        var monthlyShop1 = await context.Reports
            .Include(r => r.ReportDetails)
            .FirstOrDefaultAsync(r => r.Type == 2 && r.ShopId == 1);

        var monthlyShop2 = await context.Reports
            .Include(r => r.ReportDetails)
            .FirstOrDefaultAsync(r => r.Type == 2 && r.ShopId == 2);

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
        // Arrange
        var context = GetInMemoryDbContext();
        var repo = new ReportRepo(context);

        int shop1 = 1;
        int shop2 = 2;
        var monthStart = new DateOnly(2025, 8, 1);

        // Tạo 5 weekly report cho Shop 1
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
                ReportDetails =
            {
                new ReportDetail { ProductId = 101, Quantity = 5 }
            }
            });
        }

        // Tạo 3 weekly report cho Shop 2
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
                ReportDetails =
            {
                new ReportDetail { ProductId = 102, Quantity = 10 }
            }
            });
        }

        await context.SaveChangesAsync();

        // Act
        await repo.GenerateMonthlyReportAsync();

        // Assert Shop 1
        var monthlyShop1 = await context.Reports
            .Include(r => r.ReportDetails)
            .FirstOrDefaultAsync(r => r.Type == 2 && r.ShopId == shop1);

        Assert.NotNull(monthlyShop1);
        Assert.Equal(5000, monthlyShop1.Revenue);
        Assert.Equal(2000, monthlyShop1.Cost);
        Assert.Equal(3000, monthlyShop1.GrossProfit);
        Assert.Equal(50, monthlyShop1.OrderCounter);
        Assert.Equal(25, monthlyShop1.ReportDetails.First(d => d.ProductId == 101).Quantity);

        // Assert Shop 2
        var monthlyShop2 = await context.Reports
            .Include(r => r.ReportDetails)
            .FirstOrDefaultAsync(r => r.Type == 2 && r.ShopId == shop2);

        Assert.NotNull(monthlyShop2);
        Assert.Equal(6000, monthlyShop2.Revenue);
        Assert.Equal(2400, monthlyShop2.Cost);
        Assert.Equal(3600, monthlyShop2.GrossProfit);
        Assert.Equal(60, monthlyShop2.OrderCounter);
        Assert.Equal(30, monthlyShop2.ReportDetails.First(d => d.ProductId == 102).Quantity);
    }



}
