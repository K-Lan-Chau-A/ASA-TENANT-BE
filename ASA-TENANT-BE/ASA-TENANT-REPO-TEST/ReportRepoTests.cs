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
    1️) GenerateWeeklyReportAsync_ShouldCreateWeeklyReportFromShifts
       - Kiểm tra tạo báo cáo tuần từ các shift thực tế.
       - Bao gồm Revenue, Cost, GrossProfit, OrderCounter, ReportDetails.
    */

    /*
    2️) GenerateMonthlyReportAsync_ShouldAggregateWeeklyReports
       - Kiểm tra tạo báo cáo tháng từ các weekly report có sẵn.
       - Kiểm tra tổng hợp Revenue, Cost, GrossProfit, OrderCounter, ReportDetails.
    */

    /*
    3️) GenerateMonthlyReportAsync_ShouldAggregate4WeeklyReports
       - Kiểm tra gộp nhiều weekly report (4 tuần) trong tháng.
       - Kiểm tra tổng hợp giá trị tăng dần theo tuần.
    */

    /*
    4️) GenerateMonthlyReportAsync_ShouldOnlyAggregateSameShopId
       - Kiểm tra monthly report chỉ gộp các weekly report cùng ShopId.
       - Tránh dữ liệu của shop khác bị lẫn.
    */

    /*
    5️) GenerateMonthlyReportAsync_ShouldHandleMonthWith5WeeklyReports
       - Kiểm tra tháng có 5 tuần với số weekly report khác nhau giữa các shop.
       - Đảm bảo tổng hợp chính xác cho từng shop.
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
        var monthStart = new DateTime(2025, 9, 1);
        var nextMonthStart = monthStart.AddMonths(1);

        // Giả lập 2 weekly report trong tháng 9
        var weekly1 = new Report
        {
            ReportId = 1,
            Type = 1, // WEEKLY
            ShopId = shopId,
            StartDate = DateOnly.FromDateTime(new DateTime(2025, 9, 1)),
            EndDate = DateOnly.FromDateTime(new DateTime(2025, 9, 7)),
            Revenue = 1000,
            Cost = 400,
            GrossProfit = 600,
            OrderCounter = 10,
            ReportDetails =
            {
                new ReportDetail { ProductId = 101, Quantity = 5 },
                new ReportDetail { ProductId = 102,  Quantity = 5 }
            }
        };

        var weekly2 = new Report
        {
            ReportId = 2,
            Type = 1,
            ShopId = shopId,
            StartDate = DateOnly.FromDateTime(new DateTime(2025, 9, 1)),
            EndDate = DateOnly.FromDateTime(new DateTime(2025, 9, 7)),
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

        // Tạo 4 weekly report trong tháng 9
        for (int i = 0; i < 4; i++)
        {
            context.Reports.Add(new Report
            {
                Type = 1, // WEEKLY
                ShopId = shopId,
                StartDate = DateOnly.FromDateTime(new DateTime(2025, 9, 1).AddDays(i * 7)),
                EndDate = DateOnly.FromDateTime(new DateTime(2025, 9, 7).AddDays(i * 7)),
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

        // Tổng revenue = 1000 + 2000 + 3000 + 4000 = 10000
        Assert.Equal(10000, monthly.Revenue);

        // Tổng cost = 500 + 1000 + 1500 + 2000 = 5000
        Assert.Equal(5000, monthly.Cost);

        // Tổng profit = 500 + 1000 + 1500 + 2000 = 5000
        Assert.Equal(5000, monthly.GrossProfit);

        // Tổng order = 10 + 20 + 30 + 40 = 100
        Assert.Equal(100, monthly.OrderCounter);

        // Quantity của product 101 = 5 + 10 + 15 + 20 = 50
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
            StartDate = DateOnly.FromDateTime(new DateTime(2025, 9, 1)),
            EndDate = DateOnly.FromDateTime(new DateTime(2025, 9, 7)),
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
            StartDate = DateOnly.FromDateTime(new DateTime(2025, 9, 1)),
            EndDate = DateOnly.FromDateTime(new DateTime(2025, 9, 7)),
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

        var now = DateTime.Now; // tháng hiện tại theo môi trường test
        var monthStart = new DateOnly(now.Year, now.Month, 1);

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
        Assert.Equal(5 * 1000, monthlyShop1.Revenue); // 5000
        Assert.Equal(5 * 400, monthlyShop1.Cost);    // 2000
        Assert.Equal(5 * 600, monthlyShop1.GrossProfit); // 3000
        Assert.Equal(5 * 10, monthlyShop1.OrderCounter); // 50
        Assert.Equal(5 * 5, monthlyShop1.ReportDetails.First(d => d.ProductId == 101).Quantity); // 25

        // Assert Shop 2
        var monthlyShop2 = await context.Reports
            .Include(r => r.ReportDetails)
            .FirstOrDefaultAsync(r => r.Type == 2 && r.ShopId == shop2);

        Assert.NotNull(monthlyShop2);
        Assert.Equal(3 * 2000, monthlyShop2.Revenue); // 6000
        Assert.Equal(3 * 800, monthlyShop2.Cost);    // 2400
        Assert.Equal(3 * 1200, monthlyShop2.GrossProfit); // 3600
        Assert.Equal(3 * 20, monthlyShop2.OrderCounter); // 60
        Assert.Equal(3 * 10, monthlyShop2.ReportDetails.First(d => d.ProductId == 102).Quantity); // 30
    }


}
