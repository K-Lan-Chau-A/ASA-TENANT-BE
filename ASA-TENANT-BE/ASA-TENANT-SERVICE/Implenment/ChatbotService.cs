using ASA_TENANT_REPO.Models;
using ASA_TENANT_REPO.Repository;
using ASA_TENANT_SERVICE.DTOs;
using ASA_TENANT_SERVICE.Interface;
using ASA_TENANT_SERVICE.Templates;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace ASA_TENANT_SERVICE.Implenment
{
    public class ChatbotService : IChatbotService
    {
        private readonly ShopRepo _shopRepo;
        private readonly OrderRepo _orderRepo;
        private readonly CustomerRepo _customerRepo;
        private readonly ProductRepo _productRepo;
        private readonly OrderDetailRepo _orderDetailRepo;
        private readonly TransactionRepo _transactionRepo;
        private readonly CategoryRepo _categoryRepo;
        private readonly RankRepo _rankRepo;
        private readonly IGeminiService _geminiService;
        private readonly ILogger<ChatbotService> _logger;

        public ChatbotService(
            ShopRepo shopRepo,
            OrderRepo orderRepo,
            CustomerRepo customerRepo,
            ProductRepo productRepo,
            OrderDetailRepo orderDetailRepo,
            TransactionRepo transactionRepo,
            CategoryRepo categoryRepo,
            RankRepo rankRepo,
            IGeminiService geminiService,
            ILogger<ChatbotService> logger)
        {
            _shopRepo = shopRepo;
            _orderRepo = orderRepo;
            _customerRepo = customerRepo;
            _productRepo = productRepo;
            _orderDetailRepo = orderDetailRepo;
            _transactionRepo = transactionRepo;
            _categoryRepo = categoryRepo;
            _rankRepo = rankRepo;
            _geminiService = geminiService;
            _logger = logger;
        }

        public async Task<ChatbotResponseDto> ProcessQuestionAsync(long shopId, string question)
        {
            try
            {
                var analysisType = DetermineAnalysisType(question);
                var answer = await GenerateAnswerAsync(shopId, question, analysisType);
                
                return new ChatbotResponseDto
                {
                    Question = question,
                    Answer = answer,
                    AnalysisType = analysisType,
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chatbot question for shop {ShopId}", shopId);
                return new ChatbotResponseDto
                {
                    Question = question,
                    Answer = "Xin lỗi, tôi gặp sự cố khi xử lý câu hỏi của bạn. Vui lòng thử lại sau.",
                    AnalysisType = "error",
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        private string DetermineAnalysisType(string question)
        {
            // Use Gemini's question type determination for better accuracy
            return GeminiPrompts.QuestionTypes.DetermineQuestionType(question);
        }

        private bool ContainsKeywords(string text, string[] keywords)
        {
            return keywords.Any(keyword => text.Contains(keyword));
        }

        private async Task<string> GenerateAnswerAsync(long shopId, string question, string analysisType)
        {
            try
            {
                // Always use comprehensive analysis for all questions to provide best AI responses
                return await GenerateComprehensiveAnswerAsync(shopId, question, analysisType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI response for shop {ShopId}, falling back to hardcoded responses", shopId);
                
                // Fallback to hardcoded responses if Gemini fails
                return await GenerateFallbackAnswerAsync(shopId, question, analysisType);
            }
        }

        private bool IsComplexQuestion(string questionLower)
        {
            // Check for multiple topic indicators
            var topicKeywords = new[]
            {
                new[] { "chiến lược", "strategy", "cách tăng", "gợi ý" },
                new[] { "sản phẩm", "product", "hàng hóa", "tạp hóa" },
                new[] { "doanh thu", "revenue", "tiền", "lợi nhuận" },
                new[] { "khách hàng", "customer", "member", "thành viên" },
                new[] { "tồn kho", "inventory", "stock", "hàng" }
            };

            var matchedTopics = topicKeywords.Count(topic => 
                topic.Any(keyword => questionLower.Contains(keyword)));

            // If 2 or more topics are mentioned, it's a complex question
            return matchedTopics >= 2;
        }

        private async Task<string> GenerateComprehensiveAnswerAsync(long shopId, string question, string analysisType)
        {
            try
            {
                // Gather comprehensive data for all questions
                var strategyData = await GetStrategyAnalyticsAsync(shopId);
                var productData = await GetProductSuggestionDataAsync(shopId);
                var revenueData = await GetRevenueAnalyticsAsync(shopId);
                var customerData = await GetCustomerAnalyticsAsync(shopId);
                var inventoryData = await GetInventoryAnalyticsAsync(shopId);
                var shopData = await GetShopAnalyticsAsync(shopId);

                // Create comprehensive data object with all shop information
                var comprehensiveData = new ComprehensiveAnalysisDto
                {
                    ShopId = shopId,
                    ShopName = strategyData.ShopName,
                    ShopData = shopData,
                    StrategyData = strategyData,
                    ProductData = productData,
                    RevenueData = revenueData,
                    CustomerData = customerData,
                    InventoryData = inventoryData,
                    Question = question,
                    AnalysisType = analysisType
                };

                // Use Gemini AI with comprehensive prompt
                return await _geminiService.GenerateComprehensiveAnalysisAsync(shopId, question, comprehensiveData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating comprehensive analysis for shop {ShopId}", shopId);
                
                // Fallback to primary analysis type
                return await GenerateFallbackAnswerAsync(shopId, question, analysisType);
            }
        }

        private async Task<string> GenerateFallbackAnswerAsync(long shopId, string question, string analysisType)
        {
            switch (analysisType)
            {
                case "strategy":
                    var strategyData = await GetStrategyAnalyticsAsync(shopId);
                    return GenerateStrategyAnswer(question, strategyData);
                
                case "product_suggestion":
                    var suggestionData = await GetProductSuggestionDataAsync(shopId);
                    return GenerateProductSuggestionAnswer(question, suggestionData);
                
                case "revenue":
                    var revenueData = await GetRevenueAnalyticsAsync(shopId);
                    return GenerateRevenueAnswer(question, revenueData);
                
                case "customer":
                    var customerData = await GetCustomerAnalyticsAsync(shopId);
                    return GenerateCustomerAnswer(question, customerData);
                
                case "inventory":
                    var inventoryData = await GetInventoryAnalyticsAsync(shopId);
                    return GenerateInventoryAnswer(question, inventoryData);
                
                case "product":
                    var productData = await GetProductPerformanceAsync(shopId);
                    return GenerateProductAnswer(question, productData);
                
                default:
                    var shopData = await GetShopAnalyticsAsync(shopId);
                    return GenerateGeneralAnswer(question, shopData);
            }
        }

        public async Task<ShopAnalyticsDto> GetShopAnalyticsAsync(long shopId)
        {
            var shop = await _shopRepo.GetByIdAsync(shopId);
            if (shop == null)
                throw new ArgumentException($"Shop with ID {shopId} not found");

            var orders = await _orderRepo.GetByShopIdAsync(shopId);
            var customers = await _customerRepo.GetByShopIdAsync(shopId);
            var products = await _productRepo.GetByShopIdAsync(shopId);
            var orderDetails = await _orderDetailRepo.GetByShopIdAsync(shopId);

            var completedOrders = orders.Where(o => o.Status == 1).ToList();
            var totalRevenue = completedOrders.Sum(o => o.FinalPrice ?? 0);
            var totalProfit = orderDetails
                .Where(od => completedOrders.Any(o => o.OrderId == od.OrderId))
                .Sum(od => od.Profit);
            var totalOrders = orders.Count();
            var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            return new ShopAnalyticsDto
            {
                ShopId = shopId,
                ShopName = shop.ShopName ?? "Unknown",
                CreatedAt = shop.CreatedAt ?? DateTime.MinValue,
                TotalProducts = products.Count,
                TotalCustomers = customers.Count,
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                TotalProfit = totalProfit,
                AverageOrderValue = averageOrderValue,
                Status = shop.Status == 1 ? "Active" : "Inactive"
            };
        }

        public async Task<CustomerAnalyticsDto> GetCustomerAnalyticsAsync(long shopId)
        {
            var customers = await _customerRepo.GetByShopIdAsync(shopId);
            var orders = await _orderRepo.GetByShopIdAsync(shopId);
            var ranks = await _rankRepo.GetByShopIdAsync(shopId);

            var memberCustomers = customers.Where(c => c.RankId.HasValue).ToList();
            var nonMemberCustomers = customers.Where(c => !c.RankId.HasValue).ToList();
            
            var totalCustomers = customers.Count();
            var memberPercentage = totalCustomers > 0 ? (double)memberCustomers.Count / totalCustomers * 100 : 0;
            var nonMemberPercentage = 100 - memberPercentage;

            var thisMonth = DateTime.Now.AddMonths(-1);
            var newCustomersThisMonth = customers.Count(c => c.CreatedAt >= thisMonth);
            
            var customersWithOrders = customers.Where(c => orders.Any(o => o.CustomerId == c.CustomerId)).ToList();
            var returningCustomers = customersWithOrders.Count();

            var customersByRank = customers
                .Where(c => c.RankId.HasValue)
                .GroupBy(c => c.Rank?.RankName ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Count());

            var customersByGender = customers
                .Where(c => c.Gender.HasValue)
                .GroupBy(c => c.Gender == 1 ? "Nam" : c.Gender == 2 ? "Nữ" : "Khác")
                .ToDictionary(g => g.Key, g => g.Count());

            var averageCustomerSpent = customers.Any() ? customers.Average(c => c.Spent ?? 0) : 0;

            var topCustomers = new CustomerSegmentDto
            {
                TopSpenders = customers
                    .OrderByDescending(c => c.Spent ?? 0)
                    .Take(5)
                    .Select(c => new CustomerSummaryDto
                    {
                        CustomerId = c.CustomerId,
                        FullName = c.FullName ?? "Unknown",
                        Phone = c.Phone ?? "",
                        RankName = c.Rank?.RankName ?? "Chưa có hạng",
                        TotalSpent = c.Spent ?? 0,
                        TotalOrders = orders.Count(o => o.CustomerId == c.CustomerId),
                        LastOrderDate = orders.Where(o => o.CustomerId == c.CustomerId).Max(o => o.Datetime),
                        Gender = c.Gender == 1 ? "Nam" : c.Gender == 2 ? "Nữ" : "Khác"
                    })
                    .ToList(),

                MostFrequent = customers
                    .Select(c => new
                    {
                        Customer = c,
                        OrderCount = orders.Count(o => o.CustomerId == c.CustomerId)
                    })
                    .OrderByDescending(x => x.OrderCount)
                    .Take(5)
                    .Select(x => new CustomerSummaryDto
                    {
                        CustomerId = x.Customer.CustomerId,
                        FullName = x.Customer.FullName ?? "Unknown",
                        Phone = x.Customer.Phone ?? "",
                        RankName = x.Customer.Rank?.RankName ?? "Chưa có hạng",
                        TotalSpent = x.Customer.Spent ?? 0,
                        TotalOrders = x.OrderCount,
                        LastOrderDate = orders.Where(o => o.CustomerId == x.Customer.CustomerId).Max(o => o.Datetime),
                        Gender = x.Customer.Gender == 1 ? "Nam" : x.Customer.Gender == 2 ? "Nữ" : "Khác"
                    })
                    .ToList(),

                RecentCustomers = customers
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(5)
                    .Select(c => new CustomerSummaryDto
                    {
                        CustomerId = c.CustomerId,
                        FullName = c.FullName ?? "Unknown",
                        Phone = c.Phone ?? "",
                        RankName = c.Rank?.RankName ?? "Chưa có hạng",
                        TotalSpent = c.Spent ?? 0,
                        TotalOrders = orders.Count(o => o.CustomerId == c.CustomerId),
                        LastOrderDate = orders.Where(o => o.CustomerId == c.CustomerId).Max(o => o.Datetime),
                        Gender = c.Gender == 1 ? "Nam" : c.Gender == 2 ? "Nữ" : "Khác"
                    })
                    .ToList()
            };

            return new CustomerAnalyticsDto
            {
                ShopId = shopId,
                TotalCustomers = totalCustomers,
                MemberCustomers = memberCustomers.Count,
                NonMemberCustomers = nonMemberCustomers.Count,
                MemberPercentage = memberPercentage,
                NonMemberPercentage = nonMemberPercentage,
                NewCustomersThisMonth = newCustomersThisMonth,
                ReturningCustomers = returningCustomers,
                CustomersByRank = customersByRank,
                CustomersByGender = customersByGender,
                AverageCustomerSpent = averageCustomerSpent,
                TopCustomers = topCustomers
            };
        }

        public async Task<InventoryAnalyticsDto> GetInventoryAnalyticsAsync(long shopId)
        {
            var products = await _productRepo.GetByShopIdAsync(shopId);
            var categories = await _categoryRepo.GetByShopIdAsync(shopId);
            var orderDetails = await _orderDetailRepo.GetByShopIdAsync(shopId);

            var lowStockProducts = products.Where(p => p.Quantity <= p.IsLow && p.Quantity > 0).ToList();
            var outOfStockProducts = products.Where(p => p.Quantity <= 0).ToList();
            var inStockProducts = products.Where(p => p.Quantity > p.IsLow).ToList();

            var totalInventoryValue = products.Sum(p => (p.Quantity ?? 0) * (p.Cost ?? 0));

            var lowStockItems = lowStockProducts
                .Select(p => new ProductStockDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName ?? "Unknown",
                    CategoryName = p.Category?.CategoryName ?? "Chưa phân loại",
                    CurrentStock = p.Quantity ?? 0,
                    IsLow = p.IsLow ?? 0,
                    Price = p.Price ?? 0m,
                    Cost = p.Cost ?? 0m,
                    ProfitMargin = (p.Price ?? 0m) > 0 && (p.Cost ?? 0m) > 0 ? (((p.Price ?? 0m) - (p.Cost ?? 0m)) / (p.Price ?? 0m) * 100) : 0m,
                    TotalSold = orderDetails.Where(od => od.ProductId == p.ProductId).Sum(od => od.Quantity),
                    TotalRevenue = orderDetails.Where(od => od.ProductId == p.ProductId).Sum(od => od.FinalPrice),
                    StockStatus = p.Quantity <= 0 ? "Out of Stock" : p.Quantity <= p.IsLow ? "Low Stock" : "In Stock"
                })
                .ToList();

            var topSellingProducts = products
                .Select(p => new
                {
                    Product = p,
                    TotalSold = orderDetails.Where(od => od.ProductId == p.ProductId).Sum(od => od.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(10)
                .Select(x => new ProductStockDto
                {
                    ProductId = x.Product.ProductId,
                    ProductName = x.Product.ProductName ?? "Unknown",
                    CategoryName = x.Product.Category?.CategoryName ?? "Chưa phân loại",
                    CurrentStock = x.Product.Quantity ?? 0,
                    IsLow = x.Product.IsLow ?? 0,
                    Price = x.Product.Price ?? 0m,
                    Cost = x.Product.Cost ?? 0m,
                    ProfitMargin = (x.Product.Price ?? 0m) > 0 && (x.Product.Cost ?? 0m) > 0 ? (((x.Product.Price ?? 0m) - (x.Product.Cost ?? 0m)) / (x.Product.Price ?? 0m) * 100) : 0m,
                    TotalSold = x.TotalSold,
                    TotalRevenue = orderDetails.Where(od => od.ProductId == x.Product.ProductId).Sum(od => od.FinalPrice),
                    StockStatus = x.Product.Quantity <= 0 ? "Out of Stock" : x.Product.Quantity <= x.Product.IsLow ? "Low Stock" : "In Stock"
                })
                .ToList();

            var slowMovingProducts = products
                .Select(p => new
                {
                    Product = p,
                    TotalSold = orderDetails.Where(od => od.ProductId == p.ProductId).Sum(od => od.Quantity)
                })
                .Where(x => x.TotalSold < 5) // Products with less than 5 units sold
                .OrderBy(x => x.TotalSold)
                .Take(10)
                .Select(x => new ProductStockDto
                {
                    ProductId = x.Product.ProductId,
                    ProductName = x.Product.ProductName ?? "Unknown",
                    CategoryName = x.Product.Category?.CategoryName ?? "Chưa phân loại",
                    CurrentStock = x.Product.Quantity ?? 0,
                    IsLow = x.Product.IsLow ?? 0,
                    Price = x.Product.Price ?? 0m,
                    Cost = x.Product.Cost ?? 0m,
                    ProfitMargin = (x.Product.Price ?? 0m) > 0 && (x.Product.Cost ?? 0m) > 0 ? (((x.Product.Price ?? 0m) - (x.Product.Cost ?? 0m)) / (x.Product.Price ?? 0m) * 100) : 0m,
                    TotalSold = x.TotalSold,
                    TotalRevenue = orderDetails.Where(od => od.ProductId == x.Product.ProductId).Sum(od => od.FinalPrice),
                    StockStatus = x.Product.Quantity <= 0 ? "Out of Stock" : x.Product.Quantity <= x.Product.IsLow ? "Low Stock" : "In Stock"
                })
                .ToList();

            var productsByCategory = products
                .GroupBy(p => p.Category?.CategoryName ?? "Chưa phân loại")
                .ToDictionary(g => g.Key, g => g.Count());

            return new InventoryAnalyticsDto
            {
                ShopId = shopId,
                TotalProducts = products.Count,
                LowStockProducts = lowStockProducts.Count,
                OutOfStockProducts = outOfStockProducts.Count,
                InStockProducts = inStockProducts.Count,
                TotalInventoryValue = totalInventoryValue,
                LowStockItems = lowStockItems,
                TopSellingProducts = topSellingProducts,
                SlowMovingProducts = slowMovingProducts,
                ProductsByCategory = productsByCategory
            };
        }

        public async Task<RevenueAnalyticsDto> GetRevenueAnalyticsAsync(long shopId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var orders = await _orderRepo.GetByShopIdAsync(shopId);
            var transactions = await _transactionRepo.GetByShopIdAsync(shopId);
            var orderDetails = await _orderDetailRepo.GetByShopIdAsync(shopId);

            var completedOrders = orders.Where(o => o.Status == 1).ToList();
            var today = DateTime.Today;
            var thisWeekStart = today.AddDays(-(int)today.DayOfWeek);
            var thisMonthStart = new DateTime(today.Year, today.Month, 1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);
            var lastMonthEnd = thisMonthStart.AddDays(-1);

            var totalRevenue = completedOrders.Sum(o => o.FinalPrice ?? 0);
            var todayRevenue = completedOrders.Where(o => o.Datetime?.Date == today).Sum(o => o.FinalPrice ?? 0);
            var thisWeekRevenue = completedOrders.Where(o => o.Datetime >= thisWeekStart).Sum(o => o.FinalPrice ?? 0);
            var thisMonthRevenue = completedOrders.Where(o => o.Datetime >= thisMonthStart).Sum(o => o.FinalPrice ?? 0);
            var lastMonthRevenue = completedOrders.Where(o => o.Datetime >= lastMonthStart && o.Datetime <= lastMonthEnd).Sum(o => o.FinalPrice ?? 0);

            var revenueGrowth = lastMonthRevenue > 0 ? ((thisMonthRevenue - lastMonthRevenue) / lastMonthRevenue * 100) : 0;

            // Calculate profits
            var totalProfit = orderDetails
                .Where(od => completedOrders.Any(o => o.OrderId == od.OrderId))
                .Sum(od => od.Profit);
            var todayProfit = orderDetails
                .Where(od => completedOrders.Any(o => o.OrderId == od.OrderId && o.Datetime?.Date == today))
                .Sum(od => od.Profit);
            var thisWeekProfit = orderDetails
                .Where(od => completedOrders.Any(o => o.OrderId == od.OrderId && o.Datetime >= thisWeekStart))
                .Sum(od => od.Profit);
            var thisMonthProfit = orderDetails
                .Where(od => completedOrders.Any(o => o.OrderId == od.OrderId && o.Datetime >= thisMonthStart))
                .Sum(od => od.Profit);
            var lastMonthProfit = orderDetails
                .Where(od => completedOrders.Any(o => o.OrderId == od.OrderId && o.Datetime >= lastMonthStart && o.Datetime <= lastMonthEnd))
                .Sum(od => od.Profit);

            var totalOrders = completedOrders.Count();
            var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            var todayOrders = completedOrders.Count(o => o.Datetime?.Date == today);
            var thisWeekOrders = completedOrders.Count(o => o.Datetime >= thisWeekStart);
            var thisMonthOrders = completedOrders.Count(o => o.Datetime >= thisMonthStart);

            var revenueByPaymentMethod = completedOrders
                .GroupBy(o => o.PaymentMethod ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Sum(o => o.FinalPrice ?? 0));

            var revenueByCategory = orderDetails
                .Join(completedOrders, od => od.OrderId, o => o.OrderId, (od, o) => new { od, o })
                .GroupBy(x => x.od.Product?.Category?.CategoryName ?? "Chưa phân loại")
                .ToDictionary(g => g.Key, g => g.Sum(x => x.od.FinalPrice));

            var dailyRevenue = completedOrders
                .Where(o => o.Datetime >= thisMonthStart)
                .GroupBy(o => o.Datetime?.Date)
                .Select(g => new DailyRevenueDto
                {
                    Date = g.Key ?? DateTime.MinValue,
                    Revenue = g.Sum(o => o.FinalPrice ?? 0),
                    OrderCount = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();

            var monthlyRevenue = completedOrders
                .GroupBy(o => new { o.Datetime?.Year, o.Datetime?.Month })
                .Select(g => new MonthlyRevenueDto
                {
                    Year = g.Key.Year ?? DateTime.Now.Year,
                    Month = g.Key.Month ?? DateTime.Now.Month,
                    MonthName = g.Key.Month.HasValue ? new DateTime(g.Key.Year ?? DateTime.Now.Year, g.Key.Month.Value, 1).ToString("MM/yyyy") : "",
                    Revenue = g.Sum(o => o.FinalPrice ?? 0),
                    OrderCount = g.Count()
                })
                .OrderByDescending(m => m.Year).ThenByDescending(m => m.Month)
                .Take(12)
                .ToList();

            return new RevenueAnalyticsDto
            {
                ShopId = shopId,
                TotalRevenue = totalRevenue,
                TotalProfit = totalProfit,
                TodayRevenue = todayRevenue,
                TodayProfit = todayProfit,
                ThisWeekRevenue = thisWeekRevenue,
                ThisWeekProfit = thisWeekProfit,
                ThisMonthRevenue = thisMonthRevenue,
                ThisMonthProfit = thisMonthProfit,
                LastMonthRevenue = lastMonthRevenue,
                LastMonthProfit = lastMonthProfit,
                RevenueGrowth = revenueGrowth,
                AverageOrderValue = averageOrderValue,
                TotalOrders = totalOrders,
                TodayOrders = todayOrders,
                ThisWeekOrders = thisWeekOrders,
                ThisMonthOrders = thisMonthOrders,
                RevenueByPaymentMethod = revenueByPaymentMethod,
                RevenueByCategory = revenueByCategory,
                DailyRevenue = dailyRevenue,
                MonthlyRevenue = monthlyRevenue
            };
        }

        public async Task<StrategyAnalyticsDto> GetStrategyAnalyticsAsync(long shopId)
        {
            // Gather comprehensive data for strategy analysis
            var shop = await _shopRepo.GetByIdAsync(shopId);
            var orders = await _orderRepo.GetByShopIdAsync(shopId);
            var customers = await _customerRepo.GetByShopIdAsync(shopId);
            var products = await _productRepo.GetByShopIdAsync(shopId);
            var orderDetails = await _orderDetailRepo.GetByShopIdAsync(shopId);
            var transactions = await _transactionRepo.GetByShopIdAsync(shopId);

            var completedOrders = orders.Where(o => o.Status == 1).ToList();
            var totalRevenue = completedOrders.Sum(o => o.FinalPrice ?? 0);
            var totalProfit = orderDetails
                .Where(od => completedOrders.Any(o => o.OrderId == od.OrderId))
                .Sum(od => od.Profit);
            var totalOrders = completedOrders.Count();
            var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            var today = DateTime.Today;
            var thisMonthStart = new DateTime(today.Year, today.Month, 1);
            var thisMonthRevenue = completedOrders.Where(o => o.Datetime >= thisMonthStart).Sum(o => o.FinalPrice ?? 0);
            var thisMonthProfit = orderDetails
                .Where(od => completedOrders.Any(o => o.OrderId == od.OrderId && o.Datetime >= thisMonthStart))
                .Sum(od => od.Profit);
            var lastMonthStart = thisMonthStart.AddMonths(-1);
            var lastMonthRevenue = completedOrders.Where(o => o.Datetime >= lastMonthStart && o.Datetime < thisMonthStart).Sum(o => o.FinalPrice ?? 0);
            var lastMonthProfit = orderDetails
                .Where(od => completedOrders.Any(o => o.OrderId == od.OrderId && o.Datetime >= lastMonthStart && o.Datetime < thisMonthStart))
                .Sum(od => od.Profit);
            
            var revenueGrowth = lastMonthRevenue > 0 ? ((thisMonthRevenue - lastMonthRevenue) / lastMonthRevenue * 100) : 0;

            var memberCustomers = customers.Where(c => c.RankId.HasValue).Count();
            var nonMemberCustomers = customers.Count() - memberCustomers;
            var memberPercentage = customers.Any() ? (double)memberCustomers / customers.Count() * 100 : 0;

            var lowStockProducts = products.Where(p => p.IsLow == 1).Count();
            var outOfStockProducts = products.Where(p => p.Quantity <= 0).Count();

            var topSellingProducts = products
                .Select(p => new
                {
                    Product = p,
                    TotalSold = orderDetails.Where(od => od.ProductId == p.ProductId).Sum(od => od.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToList();

            var topCategories = orderDetails
                .Join(completedOrders, od => od.OrderId, o => o.OrderId, (od, o) => new { od, o })
                .GroupBy(x => x.od.Product?.Category?.CategoryName ?? "Chưa phân loại")
                .Select(g => new { Category = g.Key, Revenue = g.Sum(x => x.od.FinalPrice) })
                .OrderByDescending(x => x.Revenue)
                .Take(3)
                .ToList();

            return new StrategyAnalyticsDto
            {
                ShopId = shopId,
                ShopName = shop.ShopName ?? "Unknown",
                TotalRevenue = totalRevenue,
                TotalProfit = totalProfit,
                ThisMonthRevenue = thisMonthRevenue,
                ThisMonthProfit = thisMonthProfit,
                LastMonthRevenue = lastMonthRevenue,
                LastMonthProfit = lastMonthProfit,
                RevenueGrowth = revenueGrowth,
                AverageOrderValue = averageOrderValue,
                TotalOrders = totalOrders,
                TotalCustomers = customers.Count(),
                MemberCustomers = memberCustomers,
                NonMemberCustomers = nonMemberCustomers,
                MemberPercentage = memberPercentage,
                TotalProducts = products.Count(),
                LowStockProducts = lowStockProducts,
                OutOfStockProducts = outOfStockProducts,
                TopSellingProducts = topSellingProducts.Select(x => new ProductStrategyDto
                {
                    ProductId = x.Product.ProductId,
                    ProductName = x.Product.ProductName ?? "Unknown",
                    TotalSold = x.TotalSold,
                    Revenue = orderDetails.Where(od => od.ProductId == x.Product.ProductId).Sum(od => od.FinalPrice),
                    ProfitMargin = (x.Product.Price ?? 0) > 0 && (x.Product.Cost ?? 0) > 0 ? (((x.Product.Price ?? 0) - (x.Product.Cost ?? 0)) / (x.Product.Price ?? 0) * 100) : 0
                }).ToList(),
                TopCategories = topCategories.Select(x => new CategoryStrategyDto
                {
                    CategoryName = x.Category,
                    Revenue = x.Revenue
                }).ToList()
            };
        }

        public async Task<ProductSuggestionDto> GetProductSuggestionDataAsync(long shopId)
        {
            var shop = await _shopRepo.GetByIdAsync(shopId);
            var products = await _productRepo.GetByShopIdAsync(shopId);
            var categories = await _categoryRepo.GetByShopIdAsync(shopId);
            var orderDetails = await _orderDetailRepo.GetByShopIdAsync(shopId);
            var orders = await _orderRepo.GetByShopIdAsync(shopId);

            var completedOrders = orders.Where(o => o.Status == 1).ToList();
            
            // Analyze current product performance
            var currentProducts = products.Select(p => new
            {
                Product = p,
                    TotalSold = orderDetails.Where(od => od.ProductId == p.ProductId).Sum(od => od.Quantity),
                    TotalRevenue = orderDetails.Where(od => od.ProductId == p.ProductId).Sum(od => od.FinalPrice),
                ProfitMargin = (p.Price ?? 0) > 0 && (p.Cost ?? 0) > 0 ? (((p.Price ?? 0) - (p.Cost ?? 0)) / (p.Price ?? 0) * 100) : 0
            }).ToList();

            var topSellingProducts = currentProducts
                .OrderByDescending(x => x.TotalSold)
                .Take(10)
                .Select(x => new ProductTrendDto
                {
                    ProductId = x.Product.ProductId,
                    ProductName = x.Product.ProductName ?? "Unknown",
                    CategoryName = x.Product.Category?.CategoryName ?? "Chưa phân loại",
                    TotalSold = x.TotalSold,
                    Revenue = x.TotalRevenue,
                    ProfitMargin = x.ProfitMargin,
                    CurrentStock = x.Product.Quantity ?? 0,
                    Price = x.Product.Price ?? 0m,
                    Cost = x.Product.Cost ?? 0m
                })
                .ToList();

            var categoriesPerformance = currentProducts
                .GroupBy(x => x.Product.Category?.CategoryName ?? "Chưa phân loại")
                .Select(g => new CategoryTrendDto
                {
                    CategoryName = g.Key,
                    ProductCount = g.Count(),
                    TotalRevenue = g.Sum(x => x.TotalRevenue),
                    TotalSold = g.Sum(x => x.TotalSold),
                    AverageProfitMargin = (decimal)g.Average(x => x.ProfitMargin)
                })
                .OrderByDescending(c => c.TotalRevenue)
                .ToList();

            var lowStockProducts = currentProducts
                .Where(x => x.Product.IsLow == 1 || x.Product.Quantity <= 0)
                .Select(x => new ProductTrendDto
                {
                    ProductId = x.Product.ProductId,
                    ProductName = x.Product.ProductName ?? "Unknown",
                    CategoryName = x.Product.Category?.CategoryName ?? "Chưa phân loại",
                    TotalSold = x.TotalSold,
                    Revenue = x.TotalRevenue,
                    ProfitMargin = x.ProfitMargin,
                    CurrentStock = x.Product.Quantity ?? 0,
                    Price = x.Product.Price ?? 0m,
                    Cost = x.Product.Cost ?? 0m
                })
                .ToList();

            var slowMovingProducts = currentProducts
                .Where(x => x.TotalSold < 5)
                .OrderBy(x => x.TotalSold)
                .Take(10)
                .Select(x => new ProductTrendDto
                {
                    ProductId = x.Product.ProductId,
                    ProductName = x.Product.ProductName ?? "Unknown",
                    CategoryName = x.Product.Category?.CategoryName ?? "Chưa phân loại",
                    TotalSold = x.TotalSold,
                    Revenue = x.TotalRevenue,
                    ProfitMargin = x.ProfitMargin,
                    CurrentStock = x.Product.Quantity ?? 0,
                    Price = x.Product.Price ?? 0m,
                    Cost = x.Product.Cost ?? 0m
                })
                .ToList();

            return new ProductSuggestionDto
            {
                ShopId = shopId,
                ShopName = shop.ShopName ?? "Unknown",
                CurrentProductsCount = products.Count(),
                TopSellingProducts = topSellingProducts,
                CategoriesPerformance = categoriesPerformance,
                LowStockProducts = lowStockProducts,
                SlowMovingProducts = slowMovingProducts,
                TotalRevenue = completedOrders.Sum(o => o.FinalPrice ?? 0),
                AverageOrderValue = completedOrders.Any() ? completedOrders.Average(o => o.FinalPrice ?? 0) : 0
            };
        }

        public async Task<ProductPerformanceDto> GetProductPerformanceAsync(long shopId)
        {
            var products = await _productRepo.GetByShopIdAsync(shopId);
            var categories = await _categoryRepo.GetByShopIdAsync(shopId);
            var orderDetails = await _orderDetailRepo.GetByShopIdAsync(shopId);

            var productPerformance = products.Select(p => new
            {
                Product = p,
                    TotalSold = orderDetails.Where(od => od.ProductId == p.ProductId).Sum(od => od.Quantity),
                    TotalRevenue = orderDetails.Where(od => od.ProductId == p.ProductId).Sum(od => od.FinalPrice),
                ProfitMargin = p.Price > 0 && p.Cost > 0 ? ((p.Price - p.Cost) / p.Price * 100) : 0m
            }).ToList();

            var topSellingProducts = productPerformance
                .OrderByDescending(x => x.TotalSold)
                .Take(10)
                .Select(x => new ProductStockDto
                {
                    ProductId = x.Product.ProductId,
                    ProductName = x.Product.ProductName ?? "Unknown",
                    CategoryName = x.Product.Category?.CategoryName ?? "Chưa phân loại",
                    CurrentStock = x.Product.Quantity ?? 0,
                    IsLow = x.Product.IsLow ?? 0,
                    Price = x.Product.Price ?? 0m,
                    Cost = x.Product.Cost ?? 0m,
                    ProfitMargin = x.ProfitMargin ?? 0m,
                    TotalSold = x.TotalSold,
                    TotalRevenue = x.TotalRevenue,
                    StockStatus = x.Product.Quantity <= 0 ? "Out of Stock" : x.Product.Quantity <= x.Product.IsLow ? "Low Stock" : "In Stock"
                })
                .ToList();

            var worstSellingProducts = productPerformance
                .OrderBy(x => x.TotalSold)
                .Take(10)
                .Select(x => new ProductStockDto
                {
                    ProductId = x.Product.ProductId,
                    ProductName = x.Product.ProductName ?? "Unknown",
                    CategoryName = x.Product.Category?.CategoryName ?? "Chưa phân loại",
                    CurrentStock = x.Product.Quantity ?? 0,
                    IsLow = x.Product.IsLow ?? 0,
                    Price = x.Product.Price ?? 0m,
                    Cost = x.Product.Cost ?? 0m,
                    ProfitMargin = x.ProfitMargin ?? 0m,
                    TotalSold = x.TotalSold,
                    TotalRevenue = x.TotalRevenue,
                    StockStatus = x.Product.Quantity <= 0 ? "Out of Stock" : x.Product.Quantity <= x.Product.IsLow ? "Low Stock" : "In Stock"
                })
                .ToList();

            var mostProfitableProducts = productPerformance
                .Where(x => x.ProfitMargin > 0)
                .OrderByDescending(x => x.ProfitMargin)
                .Take(10)
                .Select(x => new ProductStockDto
                {
                    ProductId = x.Product.ProductId,
                    ProductName = x.Product.ProductName ?? "Unknown",
                    CategoryName = x.Product.Category?.CategoryName ?? "Chưa phân loại",
                    CurrentStock = x.Product.Quantity ?? 0,
                    IsLow = x.Product.IsLow ?? 0,
                    Price = x.Product.Price ?? 0m,
                    Cost = x.Product.Cost ?? 0m,
                    ProfitMargin = x.ProfitMargin ?? 0m,
                    TotalSold = x.TotalSold,
                    TotalRevenue = x.TotalRevenue,
                    StockStatus = x.Product.Quantity <= 0 ? "Out of Stock" : x.Product.Quantity <= x.Product.IsLow ? "Low Stock" : "In Stock"
                })
                .ToList();

            var productsNeedAttention = productPerformance
                .Where(x => x.Product.IsLow == 1 || x.Product.Quantity <= 0 || x.TotalSold < 3)
                .OrderBy(x => x.TotalSold)
                .Take(10)
                .Select(x => new ProductStockDto
                {
                    ProductId = x.Product.ProductId,
                    ProductName = x.Product.ProductName ?? "Unknown",
                    CategoryName = x.Product.Category?.CategoryName ?? "Chưa phân loại",
                    CurrentStock = x.Product.Quantity ?? 0,
                    IsLow = x.Product.IsLow ?? 0,
                    Price = x.Product.Price ?? 0m,
                    Cost = x.Product.Cost ?? 0m,
                    ProfitMargin = x.ProfitMargin ?? 0m,
                    TotalSold = x.TotalSold,
                    TotalRevenue = x.TotalRevenue,
                    StockStatus = x.Product.Quantity <= 0 ? "Out of Stock" : x.Product.Quantity <= x.Product.IsLow ? "Low Stock" : "In Stock"
                })
                .ToList();

            var categoryPerformance = productPerformance
                .GroupBy(x => x.Product.Category?.CategoryName ?? "Chưa phân loại")
                .Select(g => new ProductCategoryPerformanceDto
                {
                    CategoryName = g.Key,
                    ProductCount = g.Count(),
                    TotalRevenue = g.Sum(x => x.TotalRevenue),
                    AverageRevenuePerProduct = (decimal)g.Average(x => x.TotalRevenue),
                    TotalSold = g.Sum(x => x.TotalSold),
                    AverageProfitMargin = (decimal)g.Average(x => x.ProfitMargin)
                })
                .ToDictionary(c => c.CategoryName, c => c);

            return new ProductPerformanceDto
            {
                ShopId = shopId,
                TopSellingProducts = topSellingProducts,
                WorstSellingProducts = worstSellingProducts,
                MostProfitableProducts = mostProfitableProducts,
                ProductsNeedAttention = productsNeedAttention,
                CategoryPerformance = categoryPerformance
            };
        }

        // Answer generation methods
        private string GenerateRevenueAnswer(string question, RevenueAnalyticsDto data)
        {
            var questionLower = question.ToLower();
            
            if (ContainsKeywords(questionLower, new[] { "lợi nhuận", "profit", "lãi" }))
            {
                if (ContainsKeywords(questionLower, new[] { "hôm nay", "today" }))
                {
                    return $"Lợi nhuận hôm nay của cửa hàng là {data.TodayProfit:N0} VNĐ từ doanh thu {data.TodayRevenue:N0} VNĐ. " +
                           $"Tỷ lệ lợi nhuận: {(data.TodayRevenue > 0 ? (data.TodayProfit / data.TodayRevenue * 100).ToString("F1") : "0")}%.";
                }
                
                if (ContainsKeywords(questionLower, new[] { "tuần này", "this week", "tuần" }))
                {
                    return $"Lợi nhuận tuần này của cửa hàng là {data.ThisWeekProfit:N0} VNĐ từ doanh thu {data.ThisWeekRevenue:N0} VNĐ. " +
                           $"Tỷ lệ lợi nhuận: {(data.ThisWeekRevenue > 0 ? (data.ThisWeekProfit / data.ThisWeekRevenue * 100).ToString("F1") : "0")}%.";
                }
                
                if (ContainsKeywords(questionLower, new[] { "tháng này", "this month", "tháng" }))
                {
                    var profitGrowth = data.LastMonthProfit > 0 ? ((data.ThisMonthProfit - data.LastMonthProfit) / data.LastMonthProfit * 100) : 0;
                    var growthText = profitGrowth > 0 ? $"tăng {profitGrowth:F1}%" : 
                                    profitGrowth < 0 ? $"giảm {Math.Abs(profitGrowth):F1}%" : "không thay đổi";
                    return $"Lợi nhuận tháng này của cửa hàng là {data.ThisMonthProfit:N0} VNĐ từ doanh thu {data.ThisMonthRevenue:N0} VNĐ. " +
                           $"So với tháng trước, lợi nhuận {growthText}. " +
                           $"Tỷ lệ lợi nhuận: {(data.ThisMonthRevenue > 0 ? (data.ThisMonthProfit / data.ThisMonthRevenue * 100).ToString("F1") : "0")}%.";
                }
                
                return $"Tổng lợi nhuận của cửa hàng là {data.TotalProfit:N0} VNĐ từ doanh thu {data.TotalRevenue:N0} VNĐ. " +
                       $"Tỷ lệ lợi nhuận: {(data.TotalRevenue > 0 ? (data.TotalProfit / data.TotalRevenue * 100).ToString("F1") : "0")}%.";
            }
            
            if (ContainsKeywords(questionLower, new[] { "hôm nay", "today" }))
            {
                return $"Doanh thu hôm nay của cửa hàng là {data.TodayRevenue:N0} VNĐ với lợi nhuận {data.TodayProfit:N0} VNĐ từ {data.TodayOrders} đơn hàng. " +
                       $"Trung bình mỗi đơn hàng có giá trị {data.AverageOrderValue:N0} VNĐ.";
            }
            
            if (ContainsKeywords(questionLower, new[] { "tuần này", "this week", "tuần" }))
            {
                return $"Doanh thu tuần này của cửa hàng là {data.ThisWeekRevenue:N0} VNĐ với lợi nhuận {data.ThisWeekProfit:N0} VNĐ từ {data.ThisWeekOrders} đơn hàng.";
            }
            
            if (ContainsKeywords(questionLower, new[] { "tháng này", "this month", "tháng" }))
            {
                var growthText = data.RevenueGrowth > 0 ? $"tăng {data.RevenueGrowth:F1}%" : 
                                data.RevenueGrowth < 0 ? $"giảm {Math.Abs(data.RevenueGrowth):F1}%" : "không thay đổi";
                return $"Doanh thu tháng này của cửa hàng là {data.ThisMonthRevenue:N0} VNĐ với lợi nhuận {data.ThisMonthProfit:N0} VNĐ từ {data.ThisMonthOrders} đơn hàng. " +
                       $"So với tháng trước, doanh thu {growthText}.";
            }
            
            return $"Tổng doanh thu của cửa hàng là {data.TotalRevenue:N0} VNĐ với tổng lợi nhuận {data.TotalProfit:N0} VNĐ từ {data.TotalOrders} đơn hàng. " +
                   $"Trung bình mỗi đơn hàng có giá trị {data.AverageOrderValue:N0} VNĐ. " +
                   $"Tỷ lệ lợi nhuận: {(data.TotalRevenue > 0 ? (data.TotalProfit / data.TotalRevenue * 100).ToString("F1") : "0")}%.";
        }

        private string GenerateCustomerAnswer(string question, CustomerAnalyticsDto data)
        {
            var questionLower = question.ToLower();
            
            if (ContainsKeywords(questionLower, new[] { "member", "thành viên", "%" }))
            {
                return $"Cửa hàng có {data.TotalCustomers} khách hàng tổng cộng. " +
                       $"Trong đó {data.MemberPercentage:F1}% ({data.MemberCustomers} khách) là thành viên, " +
                       $"còn lại {data.NonMemberPercentage:F1}% ({data.NonMemberCustomers} khách) chưa tạo tài khoản thành viên.";
            }
            
            if (ContainsKeywords(questionLower, new[] { "khách hàng chủ yếu", "khách hàng nhiều nhất" }))
            {
                var topRank = data.CustomersByRank.OrderByDescending(x => x.Value).FirstOrDefault();
                var topGender = data.CustomersByGender.OrderByDescending(x => x.Value).FirstOrDefault();
                
                return $"Khách hàng chủ yếu của cửa hàng: " +
                       $"{(topRank.Key != null ? $"hạng {topRank.Key} ({topRank.Value} khách), " : "")}" +
                       $"{(topGender.Key != null ? $"{topGender.Key} ({topGender.Value} khách)" : "")}. " +
                       $"Trung bình mỗi khách hàng đã chi tiêu {data.AverageCustomerSpent:N0} VNĐ.";
            }
            
            if (ContainsKeywords(questionLower, new[] { "khách hàng mới", "new customer" }))
            {
                return $"Tháng này có {data.NewCustomersThisMonth} khách hàng mới đăng ký. " +
                       $"Tổng cộng có {data.ReturningCustomers} khách hàng đã từng mua hàng.";
            }
            
            return $"Cửa hàng có {data.TotalCustomers} khách hàng. " +
                   $"Trong đó {data.MemberPercentage:F1}% là thành viên và {data.NonMemberPercentage:F1}% chưa tạo tài khoản. " +
                   $"Trung bình mỗi khách hàng chi tiêu {data.AverageCustomerSpent:N0} VNĐ.";
        }

        private string GenerateInventoryAnswer(string question, InventoryAnalyticsDto data)
        {
            var questionLower = question.ToLower();
            
            if (ContainsKeywords(questionLower, new[] { "tồn kho", "stock", "hàng" }))
            {
                return $"Cửa hàng có {data.TotalProducts} sản phẩm trong kho với tổng giá trị {data.TotalInventoryValue:N0} VNĐ. " +
                       $"Trong đó {data.InStockProducts} sản phẩm còn hàng, {data.LowStockProducts} sản phẩm sắp hết hàng, " +
                       $"và {data.OutOfStockProducts} sản phẩm đã hết hàng.";
            }
            
            if (ContainsKeywords(questionLower, new[] { "sắp hết", "low stock", "cần nhập" }))
            {
                if (data.LowStockItems.Any())
                {
                    var topLowStock = data.LowStockItems.Take(3);
                    var items = string.Join(", ", topLowStock.Select(p => $"{p.ProductName} (còn {p.CurrentStock})"));
                    return $"Các sản phẩm sắp hết hàng cần nhập thêm: {items}. " +
                           $"Tổng cộng có {data.LowStockProducts} sản phẩm cần chú ý.";
                }
                return "Hiện tại không có sản phẩm nào sắp hết hàng.";
            }
            
            return $"Tình hình tồn kho: {data.TotalProducts} sản phẩm, " +
                   $"giá trị tồn kho {data.TotalInventoryValue:N0} VNĐ. " +
                   $"{data.LowStockProducts} sản phẩm cần nhập thêm.";
        }

        private string GenerateProductAnswer(string question, ProductPerformanceDto data)
        {
            var questionLower = question.ToLower();
            
            if (ContainsKeywords(questionLower, new[] { "bán chạy", "top selling", "nhiều nhất" }))
            {
                if (data.TopSellingProducts.Any())
                {
                    var topProducts = data.TopSellingProducts.Take(3);
                    var products = string.Join(", ", topProducts.Select(p => $"{p.ProductName} ({p.TotalSold} đơn vị)"));
                    return $"Sản phẩm bán chạy nhất: {products}.";
                }
                return "Chưa có dữ liệu về sản phẩm bán chạy.";
            }
            
            if (ContainsKeywords(questionLower, new[] { "ít bán", "chậm", "worst" }))
            {
                if (data.WorstSellingProducts.Any())
                {
                    var worstProducts = data.WorstSellingProducts.Take(3);
                    var products = string.Join(", ", worstProducts.Select(p => $"{p.ProductName} ({p.TotalSold} đơn vị)"));
                    return $"Sản phẩm bán chậm: {products}.";
                }
                return "Tất cả sản phẩm đều có doanh số tốt.";
            }
            
            if (ContainsKeywords(questionLower, new[] { "lợi nhuận", "profit", "hiệu quả" }))
            {
                if (data.MostProfitableProducts.Any())
                {
                    var profitableProducts = data.MostProfitableProducts.Take(3);
                    var products = string.Join(", ", profitableProducts.Select(p => $"{p.ProductName} (lợi nhuận {p.ProfitMargin:F1}%)"));
                    return $"Sản phẩm có lợi nhuận cao nhất: {products}.";
                }
                return "Chưa có dữ liệu về lợi nhuận sản phẩm.";
            }
            
            return $"Cửa hàng có {data.TopSellingProducts.Count} sản phẩm bán chạy và " +
                   $"{data.ProductsNeedAttention.Count} sản phẩm cần chú ý.";
        }

        private string GenerateStrategyAnswer(string question, StrategyAnalyticsDto data)
        {
            var questionLower = question.ToLower();
            var strategies = new List<string>();

            // Revenue growth strategies
            if (ContainsKeywords(questionLower, new[] { "doanh thu", "revenue", "tăng doanh thu", "lợi nhuận", "profit" }))
            {
                strategies.Add($"Chiến lược tăng doanh thu và lợi nhuận:");
                
                if (data.RevenueGrowth < 10)
                {
                    strategies.Add($"• Doanh thu tháng này tăng {data.RevenueGrowth:F1}%, cần cải thiện:");
                    strategies.Add($"• Tăng giá trị đơn hàng trung bình (hiện tại: {data.AverageOrderValue:N0} VNĐ)");
                    strategies.Add($"• Tăng tần suất mua hàng của khách hàng");
                }
                else
                {
                    strategies.Add($"• Doanh thu đang tăng trưởng tốt ({data.RevenueGrowth:F1}%)");
                    strategies.Add($"• Tiếp tục duy trì và mở rộng chiến lược hiện tại");
                }

                if (data.TopCategories.Any())
                {
                    strategies.Add($"• Tập trung vào danh mục bán chạy: {string.Join(", ", data.TopCategories.Take(2).Select(c => c.CategoryName))}");
                }
                
                // Add profit-specific recommendations
                var profitMargin = data.ThisMonthRevenue > 0 ? (data.ThisMonthProfit / data.ThisMonthRevenue * 100) : 0;
                strategies.Add($"• Lợi nhuận tháng này: {data.ThisMonthProfit:N0} VNĐ (tỷ lệ {profitMargin:F1}%)");
                
                if (profitMargin < 20)
                {
                    strategies.Add($"• Cần cải thiện tỷ lệ lợi nhuận bằng cách:");
                    strategies.Add($"• Tối ưu giá vốn hàng bán");
                    strategies.Add($"• Tăng giá bán cho các sản phẩm có nhu cầu cao");
                    strategies.Add($"• Giảm chi phí vận hành và quản lý");
                }
            }

            // Customer strategies
            if (ContainsKeywords(questionLower, new[] { "khách hàng", "customer", "tăng khách" }))
            {
                strategies.Add($"👥 **Chiến lược khách hàng:**");
                strategies.Add($"• Tỷ lệ thành viên hiện tại: {data.MemberPercentage:F1}% ({data.MemberCustomers}/{data.TotalCustomers})");
                
                if (data.MemberPercentage < 50)
                {
                    strategies.Add($"• Cần tăng tỷ lệ thành viên:");
                    strategies.Add($"• Tạo chương trình ưu đãi cho thành viên mới");
                    strategies.Add($"• Khuyến khích khách hàng đăng ký thành viên");
                }
                
                strategies.Add($"• Chăm sóc khách hàng thân thiết");
                strategies.Add($"• Tạo chương trình khuyến mãi theo mùa");
            }

            // Product strategies
            if (ContainsKeywords(questionLower, new[] { "sản phẩm", "product", "hàng hóa" }))
            {
                strategies.Add($"📦 **Chiến lược sản phẩm:**");
                
                if (data.TopSellingProducts.Any())
                {
                    strategies.Add($"• Sản phẩm bán chạy: {string.Join(", ", data.TopSellingProducts.Take(3).Select(p => p.ProductName))}");
                    strategies.Add($"• Tăng cường marketing cho sản phẩm bán chạy");
                    strategies.Add($"• Tạo combo sản phẩm từ các mặt hàng phổ biến");
                }

                if (data.LowStockProducts > 0)
                {
                    strategies.Add($"• ⚠️ Có {data.LowStockProducts} sản phẩm sắp hết hàng, cần nhập thêm");
                }
                
                strategies.Add($"• Phân tích sản phẩm bán chậm và điều chỉnh giá");
                strategies.Add($"• Đa dạng hóa danh mục sản phẩm");
            }

            // General business strategies
            strategies.Add($"💡 **Chiến lược tổng thể:**");
            strategies.Add($"• Tối ưu hóa trải nghiệm khách hàng");
            strategies.Add($"• Sử dụng dữ liệu để đưa ra quyết định kinh doanh");
            strategies.Add($"• Tạo chương trình loyalty program");
            strategies.Add($"• Phát triển kênh bán hàng online");
            strategies.Add($"• Tăng cường quảng cáo trên mạng xã hội");

            if (!strategies.Any())
            {
                strategies.Add($"Phân tích tình hình cửa hàng {data.ShopName}:");
                strategies.Add($"• Doanh thu tháng này: {data.ThisMonthRevenue:N0} VNĐ (tăng {data.RevenueGrowth:F1}%)");
                strategies.Add($"• Lợi nhuận tháng này: {data.ThisMonthProfit:N0} VNĐ");
                strategies.Add($"• Trung bình đơn hàng: {data.AverageOrderValue:N0} VNĐ");
                strategies.Add($"• Khách hàng thành viên: {data.MemberPercentage:F1}%");
                strategies.Add($"• Tổng sản phẩm: {data.TotalProducts}");
                
                strategies.Add($"\nGợi ý cải thiện:");
                strategies.Add($"• Tăng giá trị đơn hàng trung bình");
                strategies.Add($"• Nâng cao tỷ lệ khách hàng thành viên");
                strategies.Add($"• Tối ưu hóa quản lý tồn kho");
                strategies.Add($"• Phát triển chiến lược marketing");
            }

            return string.Join("\n", strategies);
        }

        private string GenerateProductSuggestionAnswer(string question, ProductSuggestionDto data)
        {
            var questionLower = question.ToLower();
            var suggestions = new List<string>();

            // Market trend suggestions based on current performance
            suggestions.Add($"Gợi ý sản phẩm tạp hóa hot cho cửa hàng {data.ShopName}:\n");

            // Analyze current top-selling categories
            if (data.CategoriesPerformance.Any())
            {
                suggestions.Add($"Phân tích danh mục hiện tại:");
                var topCategories = data.CategoriesPerformance.Take(3);
                foreach (var category in topCategories)
                {
                    suggestions.Add($"- {category.CategoryName}: {category.ProductCount} sản phẩm, doanh thu {category.TotalRevenue:N0} VNĐ");
                }
            }

            // Product suggestions based on current performance
            suggestions.Add($"\nSản phẩm đang bán chạy tại cửa hàng:");
            if (data.TopSellingProducts.Any())
            {
                var topProducts = data.TopSellingProducts.Take(5);
                foreach (var product in topProducts)
                {
                    suggestions.Add($"- {product.ProductName} ({product.CategoryName}) - Đã bán {product.TotalSold} đơn vị");
                }
            }

            // Market trend suggestions
            suggestions.Add($"\nGợi ý sản phẩm hot trên thị trường hiện tại:");
            
            // Food & Beverage trends
            suggestions.Add($"Đồ uống và Thực phẩm:");
            suggestions.Add($"- Nước uống có ga không đường (Coca Zero, Pepsi Zero)");
            suggestions.Add($"- Trà sữa các loại (trà sữa matcha, trà sữa trân châu)");
            suggestions.Add($"- Nước tăng lực (Red Bull, Sting, Number One)");
            suggestions.Add($"- Bánh kẹo nhập khẩu (bánh Oreo, kẹo Mentos)");
            suggestions.Add($"- Snack cao cấp (Pringles, Lay's, Cheetos)");

            // Health & Wellness trends
            suggestions.Add($"\nSản phẩm sức khỏe và dinh dưỡng:");
            suggestions.Add($"- Nước suối đóng chai (Aquafina, Dasani, Lavie)");
            suggestions.Add($"- Sữa tươi các loại (Vinamilk, TH True Milk)");
            suggestions.Add($"- Ngũ cốc ăn sáng (Kellogg's, Nestle)");
            suggestions.Add($"- Bánh mì sandwich, bánh mì nguyên cám");
            suggestions.Add($"- Trái cây sấy khô, hạt dinh dưỡng");

            // Convenience items
            suggestions.Add($"\nSản phẩm tiện lợi:");
            suggestions.Add($"- Mì ăn liền cao cấp (Shin Ramyun, Indomie)");
            suggestions.Add($"- Thức ăn nhanh đóng hộp");
            suggestions.Add($"- Gia vị nấu ăn (nước mắm, tương ớt, mayonnaise)");
            suggestions.Add($"- Đồ đông lạnh (chả cá, chả lụa)");
            suggestions.Add($"- Sản phẩm vệ sinh cá nhân (khẩu trang, nước rửa tay)");

            // Seasonal suggestions
            suggestions.Add($"\nSản phẩm theo mùa (hiện tại - {DateTime.Now.Month}/2024):");
            if (DateTime.Now.Month >= 10 || DateTime.Now.Month <= 2)
            {
                suggestions.Add($"- Đồ uống nóng (cà phê hòa tan, trà túi lọc)");
                suggestions.Add($"- Thực phẩm giữ ấm (cháo ăn liền, súp)");
                suggestions.Add($"- Bánh kẹo Tết (kẹo dẻo, bánh quy)");
            }
            else
            {
                suggestions.Add($"- Đồ uống mát lạnh (nước đá, kem)");
                suggestions.Add($"- Trái cây tươi (dưa hấu, cam, táo)");
                suggestions.Add($"- Đồ ăn nhẹ mùa hè (kem, yaourt)");
            }

            // Recommendations based on current inventory
            if (data.LowStockProducts.Any())
            {
                suggestions.Add($"\nSản phẩm cần bổ sung ngay:");
                var lowStock = data.LowStockProducts.Take(3);
                foreach (var product in lowStock)
                {
                    suggestions.Add($"- {product.ProductName} (hiện còn {product.CurrentStock} đơn vị)");
                }
            }

            // Profit optimization suggestions
            suggestions.Add($"\nGợi ý tối ưu lợi nhuận:");
            suggestions.Add($"- Tập trung vào danh mục có lợi nhuận cao nhất");
            suggestions.Add($"- Nhập thêm sản phẩm bán chạy để tránh hết hàng");
            suggestions.Add($"- Thử nghiệm 1-2 sản phẩm mới mỗi tháng");
            suggestions.Add($"- Theo dõi xu hướng trên mạng xã hội");
            suggestions.Add($"- Quan sát đối thủ cạnh tranh");

            return string.Join("\n", suggestions);
        }

        private string GenerateGeneralAnswer(string question, ShopAnalyticsDto data)
        {
            return $"Tổng quan về cửa hàng {data.ShopName}: " +
                   $"Có {data.TotalProducts} sản phẩm, {data.TotalCustomers} khách hàng, " +
                   $"đã thực hiện {data.TotalOrders} đơn hàng với tổng doanh thu {data.TotalRevenue:N0} VNĐ và tổng lợi nhuận {data.TotalProfit:N0} VNĐ. " +
                   $"Trung bình mỗi đơn hàng có giá trị {data.AverageOrderValue:N0} VNĐ.";
        }
    }
}
