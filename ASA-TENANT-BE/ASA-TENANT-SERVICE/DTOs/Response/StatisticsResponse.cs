using System;
using System.Collections.Generic;

namespace ASA_TENANT_SERVICE.DTOs.Response
{
    public class TopProductResponse
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public string Barcode { get; set; }
        public string CategoryName { get; set; }
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal AveragePrice { get; set; }
        public string ImageUrl { get; set; }
    }

    public class RevenueStatsResponse
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; }
        public int TotalOrders { get; set; }
        public int TotalProductsSold { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<DailyRevenueResponse> DailyRevenues { get; set; } = new List<DailyRevenueResponse>();
    }

    public class DailyRevenueResponse
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public decimal Profit { get; set; }
        public int OrderCount { get; set; }
        public int ProductCount { get; set; }
    }

    public class TopCategoryResponse
    {
        public long CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int ProductCount { get; set; }
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal PercentageOfTotal { get; set; }
    }

    public class StatisticsOverviewResponse
    {
        public List<TopProductResponse> TopProducts { get; set; } = new List<TopProductResponse>();
        public RevenueStatsResponse RevenueStats { get; set; }
        public List<TopCategoryResponse> TopCategories { get; set; } = new List<TopCategoryResponse>();
    }
}
