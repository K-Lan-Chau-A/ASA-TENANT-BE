using ASA_TENANT_SERVICE.Configuration;
using ASA_TENANT_SERVICE.DTOs;
using ASA_TENANT_SERVICE.Interface;
using ASA_TENANT_SERVICE.Templates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace ASA_TENANT_SERVICE.Implenment
{
    public class GeminiService : IGeminiService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeminiService> _logger;
        private readonly GeminiConfiguration _geminiConfig;
        private readonly ChatbotConfiguration _chatbotConfig;
        private readonly string _apiKey;

        public GeminiService(
            IConfiguration configuration, 
            IOptions<GeminiConfiguration> geminiConfig,
            IOptions<ChatbotConfiguration> chatbotConfig,
            ILogger<GeminiService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _geminiConfig = geminiConfig.Value;
            _chatbotConfig = chatbotConfig.Value;
            
            // Lấy API key từ environment variables hoặc configuration
            _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") 
                     ?? _geminiConfig.ApiKey 
                     ?? throw new InvalidOperationException("Gemini API key not found. Please set GEMINI_API_KEY environment variable or configure GeminiSettings:ApiKey in appsettings.json");
        }

        public async Task<GeminiResponseDto> GenerateResponseAsync(GeminiRequestDto request)
        {
            try
            {
                _logger.LogInformation("Generating Gemini response for shop {ShopId}, type: {AnalysisType}", 
                    request.ShopId, request.AnalysisType);

                var prompt = BuildPrompt(request);
                var response = await CallGeminiAPI(prompt);

                return new GeminiResponseDto
                {
                    Response = response,
                    AnalysisType = request.AnalysisType,
                    IsSuccess = true,
                    TokensUsed = EstimateTokens(prompt + response),
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Gemini response for shop {ShopId}", request.ShopId);
                return new GeminiResponseDto
                {
                    Response = GetFallbackResponse(request.AnalysisType),
                    AnalysisType = request.AnalysisType,
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    TokensUsed = 0,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        public async Task<string> GenerateShopAnalysisAsync(long shopId, string question, object shopData)
        {
            try
            {
                var request = new GeminiRequestDto
                {
                    ShopId = shopId,
                    ShopName = GetShopName(shopData),
                    AnalysisType = "general",
                    Context = question,
                    Data = ConvertToDictionary(shopData)
                };

                var response = await GenerateResponseAsync(request);
                return response.Response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating shop analysis for shop {ShopId}", shopId);
                return GetFallbackResponse("general");
            }
        }

        public async Task<string> GenerateRevenueAnalysisAsync(long shopId, string question, RevenueAnalyticsDto data)
        {
            try
            {
                var request = new GeminiRequestDto
                {
                    ShopId = shopId,
                    ShopName = "Cửa hàng", // Will be replaced with actual shop name
                    AnalysisType = "revenue",
                    Context = question,
                    Data = ConvertToDictionary(data)
                };

                var prompt = GeminiPrompts.PromptTemplates.GetRevenuePrompt(request.ShopName, shopId, data);
                var response = await CallGeminiAPI(prompt);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating revenue analysis for shop {ShopId}", shopId);
                return GetFallbackResponse("revenue");
            }
        }

        public async Task<string> GenerateCustomerAnalysisAsync(long shopId, string question, CustomerAnalyticsDto data)
        {
            try
            {
                var request = new GeminiRequestDto
                {
                    ShopId = shopId,
                    ShopName = "Cửa hàng",
                    AnalysisType = "customer",
                    Context = question,
                    Data = ConvertToDictionary(data)
                };

                var prompt = GeminiPrompts.PromptTemplates.GetCustomerPrompt(request.ShopName, shopId, data);
                var response = await CallGeminiAPI(prompt);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating customer analysis for shop {ShopId}", shopId);
                return GetFallbackResponse("customer");
            }
        }

        public async Task<string> GenerateInventoryAnalysisAsync(long shopId, string question, InventoryAnalyticsDto data)
        {
            try
            {
                var request = new GeminiRequestDto
                {
                    ShopId = shopId,
                    ShopName = "Cửa hàng",
                    AnalysisType = "inventory",
                    Context = question,
                    Data = ConvertToDictionary(data)
                };

                var prompt = GeminiPrompts.PromptTemplates.GetInventoryPrompt(request.ShopName, shopId, data);
                var response = await CallGeminiAPI(prompt);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating inventory analysis for shop {ShopId}", shopId);
                return GetFallbackResponse("inventory");
            }
        }

        public async Task<string> GenerateProductAnalysisAsync(long shopId, string question, ProductPerformanceDto data)
        {
            try
            {
                var request = new GeminiRequestDto
                {
                    ShopId = shopId,
                    ShopName = "Cửa hàng",
                    AnalysisType = "product",
                    Context = question,
                    Data = ConvertToDictionary(data)
                };

                var prompt = GeminiPrompts.PromptTemplates.GetProductPrompt(request.ShopName, shopId, data);
                var response = await CallGeminiAPI(prompt);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating product analysis for shop {ShopId}", shopId);
                return GetFallbackResponse("product");
            }
        }

        private string BuildPrompt(GeminiRequestDto request)
        {
            var sb = new StringBuilder();
            
            // Base system prompt
            sb.AppendLine(GeminiPrompts.SystemPrompts.BaseSystemPrompt
                .Replace("{ShopName}", request.ShopName)
                .Replace("{ShopId}", request.ShopId.ToString())
                .Replace("{AnalysisType}", request.AnalysisType));

            // Add context/question
            sb.AppendLine($"\nCÂU HỎI CỦA NGƯỜI DÙNG: {request.Context}");

            // Add specific analysis prompt based on type
            switch (request.AnalysisType.ToLower())
            {
                case "revenue":
                    if (request.Data != null && request.Data.TryGetValue("RevenueData", out var revenueObj))
                    {
                        sb.AppendLine($"\n{DATA_MARKER}");
                        sb.AppendLine(JsonSerializer.Serialize(revenueObj, new JsonSerializerOptions { WriteIndented = true }));
                    }
                    sb.AppendLine($"\n{GeminiPrompts.SystemPrompts.RevenueAnalysisPrompt}");
                    break;

                case "customer":
                    if (request.Data != null && request.Data.TryGetValue("CustomerData", out var customerObj))
                    {
                        sb.AppendLine($"\n{DATA_MARKER}");
                        sb.AppendLine(JsonSerializer.Serialize(customerObj, new JsonSerializerOptions { WriteIndented = true }));
                    }
                    sb.AppendLine($"\n{GeminiPrompts.SystemPrompts.CustomerAnalysisPrompt}");
                    break;

                case "inventory":
                    if (request.Data != null && request.Data.TryGetValue("InventoryData", out var inventoryObj))
                    {
                        sb.AppendLine($"\n{DATA_MARKER}");
                        sb.AppendLine(JsonSerializer.Serialize(inventoryObj, new JsonSerializerOptions { WriteIndented = true }));
                    }
                    sb.AppendLine($"\n{GeminiPrompts.SystemPrompts.InventoryAnalysisPrompt}");
                    break;

                case "product":
                    if (request.Data != null && request.Data.TryGetValue("ProductData", out var productObj))
                    {
                        sb.AppendLine($"\n{DATA_MARKER}");
                        sb.AppendLine(JsonSerializer.Serialize(productObj, new JsonSerializerOptions { WriteIndented = true }));
                    }
                    sb.AppendLine($"\n{GeminiPrompts.SystemPrompts.ProductAnalysisPrompt}");
                    break;

                case "strategy":
                    if (request.Data != null && request.Data.TryGetValue("StrategyData", out var strategyObj))
                    {
                        sb.AppendLine($"\n{DATA_MARKER}");
                        sb.AppendLine(JsonSerializer.Serialize(strategyObj, new JsonSerializerOptions { WriteIndented = true }));
                    }
                    sb.AppendLine($"\n{GeminiPrompts.SystemPrompts.StrategyAnalysisPrompt}");
                    break;

                case "product_suggestion":
                    if (request.Data != null && request.Data.TryGetValue("ProductSuggestionData", out var suggestionObj))
                    {
                        sb.AppendLine($"\n{DATA_MARKER}");
                        sb.AppendLine(JsonSerializer.Serialize(suggestionObj, new JsonSerializerOptions { WriteIndented = true }));
                    }
                    sb.AppendLine($"\n{GeminiPrompts.SystemPrompts.ProductSuggestionAnalysisPrompt}");
                    break;

                case "comprehensive":
                    if (request.Data != null)
                    {
                        sb.AppendLine($"\n{DATA_MARKER}");
                        sb.AppendLine(JsonSerializer.Serialize(request.Data, new JsonSerializerOptions { WriteIndented = true }));
                    }
                    sb.AppendLine($"\n{GeminiPrompts.SystemPrompts.ComprehensiveAnalysisPrompt}");
                    break;

                default:
                    if (request.Data != null)
                    {
                        sb.AppendLine($"\n{DATA_MARKER}");
                        sb.AppendLine(JsonSerializer.Serialize(request.Data, new JsonSerializerOptions { WriteIndented = true }));
                    }
                    sb.AppendLine($"\n{GeminiPrompts.SystemPrompts.GeneralAnalysisPrompt}");
                    break;
            }

            return sb.ToString();
        }

        private async Task<string> CallGeminiAPI(string prompt)
        {
            try
            {
                // Using REST API approach since the C# client might have issues
                using var httpClient = new HttpClient();
                
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = _geminiConfig.Temperature,
                        topK = 40,
                        topP = 0.95,
                        maxOutputTokens = _geminiConfig.MaxTokens,
                        stopSequences = new string[0]
                    },
                    safetySettings = new[]
                    {
                        new
                        {
                            category = "HARM_CATEGORY_HARASSMENT",
                            threshold = "BLOCK_MEDIUM_AND_ABOVE"
                        },
                        new
                        {
                            category = "HARM_CATEGORY_HATE_SPEECH",
                            threshold = "BLOCK_MEDIUM_AND_ABOVE"
                        },
                        new
                        {
                            category = "HARM_CATEGORY_SEXUALLY_EXPLICIT",
                            threshold = "BLOCK_MEDIUM_AND_ABOVE"
                        },
                        new
                        {
                            category = "HARM_CATEGORY_DANGEROUS_CONTENT",
                            threshold = "BLOCK_MEDIUM_AND_ABOVE"
                        }
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Gemini 2.0 uses a different endpoint
                var modelEndpoint = _geminiConfig.Model.Contains("2.0") 
                    ? "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-exp:generateContent"
                    : $"https://generativelanguage.googleapis.com/v1beta/models/{_geminiConfig.Model}:generateContent";
                
                var response = await httpClient.PostAsync(
                    $"{modelEndpoint}?key={_apiKey}",
                    content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Gemini API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                    throw new Exception($"Gemini API error: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var geminiResponse = JsonSerializer.Deserialize<GeminiApiResponse>(responseContent);

                if (geminiResponse?.candidates?.Length > 0 && 
                    geminiResponse.candidates[0].content?.parts?.Length > 0)
                {
                    return geminiResponse.candidates[0].content.parts[0].text ?? "Không thể tạo phản hồi.";
                }

                throw new Exception("Invalid response from Gemini API");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini API");
                throw;
            }
        }

        private string GetFallbackResponse(string analysisType)
        {
            return analysisType.ToLower() switch
            {
                "revenue" => "Hiện tại tôi không thể phân tích dữ liệu doanh thu. Vui lòng thử lại sau hoặc liên hệ hỗ trợ kỹ thuật.",
                "customer" => "Tôi gặp sự cố khi phân tích dữ liệu khách hàng. Hãy thử lại trong vài phút.",
                "inventory" => "Không thể truy cập thông tin tồn kho lúc này. Vui lòng thử lại sau.",
                "product" => "Dữ liệu sản phẩm tạm thời không khả dụng. Hãy thử lại sau.",
                "strategy" => "Không thể tạo chiến lược lúc này. Vui lòng thử lại sau.",
                "product_suggestion" => "Không thể đưa ra gợi ý sản phẩm lúc này. Vui lòng thử lại sau.",
                "comprehensive" => "Tôi gặp sự cố khi phân tích tổng hợp. Hãy thử hỏi câu hỏi cụ thể hơn.",
                _ => "Xin lỗi, tôi gặp sự cố khi xử lý yêu cầu của bạn. Vui lòng thử lại sau."
            };
        }

        private string GetShopName(object shopData)
        {
            if (shopData is ShopAnalyticsDto shopAnalytics)
                return shopAnalytics.ShopName;
            
            return "Cửa hàng";
        }

        private Dictionary<string, object> ConvertToDictionary(object data)
        {
            if (data == null) return new Dictionary<string, object>();

            var json = JsonSerializer.Serialize(data);
            return JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
        }

        private int EstimateTokens(string text)
        {
            // Rough estimation: 1 token ≈ 4 characters for Vietnamese text
            return text.Length / 4;
        }

        private const string DATA_MARKER = "=== DỮ LIỆU CỬA HÀNG ===";

        // Response models for Gemini API
        private class GeminiApiResponse
        {
            public GeminiCandidate[]? candidates { get; set; }
        }

        private class GeminiCandidate
        {
            public GeminiContent? content { get; set; }
        }

        public async Task<string> GenerateStrategyAnalysisAsync(long shopId, string question, StrategyAnalyticsDto data)
        {
            try
            {
                var request = new GeminiRequestDto
                {
                    ShopId = shopId,
                    ShopName = data.ShopName,
                    AnalysisType = "strategy",
                    Context = question,
                    Data = ConvertToDictionary(data)
                };

                var response = await GenerateResponseAsync(request);
                return response.Response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating strategy analysis for shop {ShopId}", shopId);
                return GetFallbackResponse("strategy");
            }
        }

        public async Task<string> GenerateProductSuggestionAsync(long shopId, string question, ProductSuggestionDto data)
        {
            try
            {
                var request = new GeminiRequestDto
                {
                    ShopId = shopId,
                    ShopName = data.ShopName,
                    AnalysisType = "product_suggestion",
                    Context = question,
                    Data = ConvertToDictionary(data)
                };

                var response = await GenerateResponseAsync(request);
                return response.Response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating product suggestion for shop {ShopId}", shopId);
                return GetFallbackResponse("product_suggestion");
            }
        }

        public async Task<string> GenerateComprehensiveAnalysisAsync(long shopId, string question, ComprehensiveAnalysisDto data)
        {
            try
            {
                var request = new GeminiRequestDto
                {
                    ShopId = shopId,
                    ShopName = data.ShopName,
                    AnalysisType = "comprehensive",
                    Context = question,
                    Data = ConvertToDictionary(data)
                };

                var response = await GenerateResponseAsync(request);
                return response.Response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating comprehensive analysis for shop {ShopId}", shopId);
                return GetFallbackResponse("comprehensive");
            }
        }

        private class GeminiContent
        {
            public GeminiPart[]? parts { get; set; }
        }

        private class GeminiPart
        {
            public string? text { get; set; }
        }
    }
}
