using ASA_TENANT_SERVICE.DTOs;

namespace ASA_TENANT_SERVICE.Interface
{
    public interface IGeminiService
    {
        Task<GeminiResponseDto> GenerateResponseAsync(GeminiRequestDto request);
        Task<string> GenerateShopAnalysisAsync(long shopId, string question, object shopData);
        Task<string> GenerateRevenueAnalysisAsync(long shopId, string question, RevenueAnalyticsDto data);
        Task<string> GenerateCustomerAnalysisAsync(long shopId, string question, CustomerAnalyticsDto data);
        Task<string> GenerateInventoryAnalysisAsync(long shopId, string question, InventoryAnalyticsDto data);
        Task<string> GenerateProductAnalysisAsync(long shopId, string question, ProductPerformanceDto data);
        Task<string> GenerateStrategyAnalysisAsync(long shopId, string question, StrategyAnalyticsDto data);
        Task<string> GenerateProductSuggestionAsync(long shopId, string question, ProductSuggestionDto data);
        Task<string> GenerateComprehensiveAnalysisAsync(long shopId, string question, ComprehensiveAnalysisDto data);
    }

    public class GeminiRequestDto
    {
        public string Prompt { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
        public Dictionary<string, object>? Data { get; set; }
        public string AnalysisType { get; set; } = string.Empty;
        public long ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
    }

    public class GeminiResponseDto
    {
        public string Response { get; set; } = string.Empty;
        public string AnalysisType { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public int TokensUsed { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
