using ASA_TENANT_SERVICE.DTOs;

namespace ASA_TENANT_SERVICE.Interface
{
    public interface IChatbotService
    {
        Task<ChatbotResponseDto> ProcessQuestionAsync(long shopId, string question);
        Task<ShopAnalyticsDto> GetShopAnalyticsAsync(long shopId);
        Task<CustomerAnalyticsDto> GetCustomerAnalyticsAsync(long shopId);
        Task<InventoryAnalyticsDto> GetInventoryAnalyticsAsync(long shopId);
        Task<RevenueAnalyticsDto> GetRevenueAnalyticsAsync(long shopId, DateTime? startDate = null, DateTime? endDate = null);
        Task<ProductPerformanceDto> GetProductPerformanceAsync(long shopId);
    }
}
