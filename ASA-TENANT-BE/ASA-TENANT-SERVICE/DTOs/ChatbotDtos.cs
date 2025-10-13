using System.ComponentModel.DataAnnotations;

namespace ASA_TENANT_SERVICE.DTOs
{
    public class ChatbotResponseDto
    {
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public string AnalysisType { get; set; } = string.Empty; // "revenue", "customer", "inventory", "product", "general"
        public Dictionary<string, object>? Data { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class ShopAnalyticsDto
    {
        public long ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int TotalProducts { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class CustomerAnalyticsDto
    {
        public long ShopId { get; set; }
        public int TotalCustomers { get; set; }
        public int MemberCustomers { get; set; }
        public int NonMemberCustomers { get; set; }
        public double MemberPercentage { get; set; }
        public double NonMemberPercentage { get; set; }
        public int NewCustomersThisMonth { get; set; }
        public int ReturningCustomers { get; set; }
        public Dictionary<string, int> CustomersByRank { get; set; } = new();
        public Dictionary<string, int> CustomersByGender { get; set; } = new();
        public decimal AverageCustomerSpent { get; set; }
        public CustomerSegmentDto TopCustomers { get; set; } = new();
    }

    public class CustomerSegmentDto
    {
        public List<CustomerSummaryDto> TopSpenders { get; set; } = new();
        public List<CustomerSummaryDto> MostFrequent { get; set; } = new();
        public List<CustomerSummaryDto> RecentCustomers { get; set; } = new();
    }

    public class CustomerSummaryDto
    {
        public long CustomerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string RankName { get; set; } = string.Empty;
        public decimal TotalSpent { get; set; }
        public int TotalOrders { get; set; }
        public DateTime? LastOrderDate { get; set; }
        public string Gender { get; set; } = string.Empty;
    }

    public class InventoryAnalyticsDto
    {
        public long ShopId { get; set; }
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public int InStockProducts { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public List<ProductStockDto> LowStockItems { get; set; } = new();
        public List<ProductStockDto> TopSellingProducts { get; set; } = new();
        public List<ProductStockDto> SlowMovingProducts { get; set; } = new();
        public Dictionary<string, int> ProductsByCategory { get; set; } = new();
    }

    public class ProductStockDto
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int IsLow { get; set; }
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public decimal ProfitMargin { get; set; }
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public string StockStatus { get; set; } = string.Empty; // "In Stock", "Low Stock", "Out of Stock"
    }

    public class RevenueAnalyticsDto
    {
        public long ShopId { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal ThisWeekRevenue { get; set; }
        public decimal ThisMonthRevenue { get; set; }
        public decimal LastMonthRevenue { get; set; }
        public decimal RevenueGrowth { get; set; } // Percentage change from last month
        public decimal AverageOrderValue { get; set; }
        public int TotalOrders { get; set; }
        public int TodayOrders { get; set; }
        public int ThisWeekOrders { get; set; }
        public int ThisMonthOrders { get; set; }
        public Dictionary<string, decimal> RevenueByPaymentMethod { get; set; } = new();
        public Dictionary<string, decimal> RevenueByCategory { get; set; } = new();
        public List<DailyRevenueDto> DailyRevenue { get; set; } = new();
        public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();
    }

    public class DailyRevenueDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class MonthlyRevenueDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class ProductPerformanceDto
    {
        public long ShopId { get; set; }
        public List<ProductStockDto> TopSellingProducts { get; set; } = new();
        public List<ProductStockDto> WorstSellingProducts { get; set; } = new();
        public List<ProductStockDto> MostProfitableProducts { get; set; } = new();
        public List<ProductStockDto> ProductsNeedAttention { get; set; } = new(); // Low stock or slow moving
        public Dictionary<string, ProductCategoryPerformanceDto> CategoryPerformance { get; set; } = new();
    }

    public class ProductCategoryPerformanceDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageRevenuePerProduct { get; set; }
        public int TotalSold { get; set; }
        public decimal AverageProfitMargin { get; set; }
    }

    public class ChatbotRequestDto
    {
        [Required]
        public string Question { get; set; } = string.Empty;
        
        public string? Context { get; set; } // Additional context for better understanding
        
        public DateTime? DateRangeStart { get; set; } // For time-specific questions
        
        public DateTime? DateRangeEnd { get; set; }
    }

    public class StrategyAnalyticsDto
    {
        public long ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public decimal ThisMonthRevenue { get; set; }
        public decimal LastMonthRevenue { get; set; }
        public decimal RevenueGrowth { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int MemberCustomers { get; set; }
        public int NonMemberCustomers { get; set; }
        public double MemberPercentage { get; set; }
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public List<ProductStrategyDto> TopSellingProducts { get; set; } = new();
        public List<CategoryStrategyDto> TopCategories { get; set; } = new();
    }

    public class ProductStrategyDto
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalSold { get; set; }
        public decimal Revenue { get; set; }
        public decimal ProfitMargin { get; set; }
    }

    public class CategoryStrategyDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
    }

    public class ProductSuggestionDto
    {
        public long ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public int CurrentProductsCount { get; set; }
        public List<ProductTrendDto> TopSellingProducts { get; set; } = new();
        public List<CategoryTrendDto> CategoriesPerformance { get; set; } = new();
        public List<ProductTrendDto> LowStockProducts { get; set; } = new();
        public List<ProductTrendDto> SlowMovingProducts { get; set; } = new();
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    public class ProductTrendDto
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int TotalSold { get; set; }
        public decimal Revenue { get; set; }
        public decimal ProfitMargin { get; set; }
        public int CurrentStock { get; set; }
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
    }

    public class CategoryTrendDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalSold { get; set; }
        public decimal AverageProfitMargin { get; set; }
    }

    public class ComprehensiveAnalysisDto
    {
        public long ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public ShopAnalyticsDto ShopData { get; set; } = new();
        public StrategyAnalyticsDto StrategyData { get; set; } = new();
        public ProductSuggestionDto ProductData { get; set; } = new();
        public RevenueAnalyticsDto RevenueData { get; set; } = new();
        public CustomerAnalyticsDto CustomerData { get; set; } = new();
        public InventoryAnalyticsDto InventoryData { get; set; } = new();
        public string Question { get; set; } = string.Empty;
        public string AnalysisType { get; set; } = string.Empty;
    }
}
