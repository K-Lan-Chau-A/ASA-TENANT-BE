using ASA_TENANT_SERVICE.DTOs;
using ASA_TENANT_SERVICE.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ASA_TENANT_BE.CustomAttribute;

namespace ASA_TENANT_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [RequireFeature(2)] // Tư vấn AI
    public class ChatbotController : ControllerBase
    {
        private readonly IChatbotService _chatbotService;
        private readonly ILogger<ChatbotController> _logger;

        public ChatbotController(IChatbotService chatbotService, ILogger<ChatbotController> logger)
        {
            _chatbotService = chatbotService;
            _logger = logger;
        }

        /// <summary>
        /// Xử lý câu hỏi từ chatbot
        /// </summary>
        /// <param name="shopId">ID của cửa hàng</param>
        /// <param name="request">Request chứa câu hỏi và context</param>
        /// <returns>Câu trả lời từ chatbot</returns>
        [HttpPost("{shopId}/ask")]
        public async Task<ActionResult<ChatbotResponseDto>> AskQuestion(long shopId, [FromBody] ChatbotRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Question))
                {
                    return BadRequest(new { message = "Câu hỏi không được để trống" });
                }

                _logger.LogInformation("Processing chatbot question for shop {ShopId}: {Question}", shopId, request.Question);

                var response = await _chatbotService.ProcessQuestionAsync(shopId, request.Question);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid shop ID: {ShopId}", shopId);
                return NotFound(new { message = "Không tìm thấy cửa hàng" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chatbot question for shop {ShopId}", shopId);
                return StatusCode(500, new { message = "Có lỗi xảy ra khi xử lý câu hỏi" });
            }
        }

        /// <summary>
        /// Lấy thông tin tổng quan về cửa hàng
        /// </summary>
        /// <param name="shopId">ID của cửa hàng</param>
        /// <returns>Thông tin tổng quan cửa hàng</returns>
        [HttpGet("{shopId}/analytics/shop")]
        public async Task<ActionResult<ShopAnalyticsDto>> GetShopAnalytics(long shopId)
        {
            try
            {
                var analytics = await _chatbotService.GetShopAnalyticsAsync(shopId);
                return Ok(analytics);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid shop ID: {ShopId}", shopId);
                return NotFound(new { message = "Không tìm thấy cửa hàng" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shop analytics for shop {ShopId}", shopId);
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy thông tin cửa hàng" });
            }
        }

        /// <summary>
        /// Lấy phân tích về khách hàng
        /// </summary>
        /// <param name="shopId">ID của cửa hàng</param>
        /// <returns>Phân tích khách hàng</returns>
        [HttpGet("{shopId}/analytics/customers")]
        public async Task<ActionResult<CustomerAnalyticsDto>> GetCustomerAnalytics(long shopId)
        {
            try
            {
                var analytics = await _chatbotService.GetCustomerAnalyticsAsync(shopId);
                return Ok(analytics);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid shop ID: {ShopId}", shopId);
                return NotFound(new { message = "Không tìm thấy cửa hàng" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer analytics for shop {ShopId}", shopId);
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy thông tin khách hàng" });
            }
        }

        /// <summary>
        /// Lấy phân tích về tồn kho
        /// </summary>
        /// <param name="shopId">ID của cửa hàng</param>
        /// <returns>Phân tích tồn kho</returns>
        [HttpGet("{shopId}/analytics/inventory")]
        public async Task<ActionResult<InventoryAnalyticsDto>> GetInventoryAnalytics(long shopId)
        {
            try
            {
                var analytics = await _chatbotService.GetInventoryAnalyticsAsync(shopId);
                return Ok(analytics);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid shop ID: {ShopId}", shopId);
                return NotFound(new { message = "Không tìm thấy cửa hàng" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inventory analytics for shop {ShopId}", shopId);
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy thông tin tồn kho" });
            }
        }

        /// <summary>
        /// Lấy phân tích về doanh thu
        /// </summary>
        /// <param name="shopId">ID của cửa hàng</param>
        /// <param name="startDate">Ngày bắt đầu (tùy chọn)</param>
        /// <param name="endDate">Ngày kết thúc (tùy chọn)</param>
        /// <returns>Phân tích doanh thu</returns>
        [HttpGet("{shopId}/analytics/revenue")]
        public async Task<ActionResult<RevenueAnalyticsDto>> GetRevenueAnalytics(
            long shopId, 
            [FromQuery] DateTime? startDate = null, 
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var analytics = await _chatbotService.GetRevenueAnalyticsAsync(shopId, startDate, endDate);
                return Ok(analytics);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid shop ID: {ShopId}", shopId);
                return NotFound(new { message = "Không tìm thấy cửa hàng" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue analytics for shop {ShopId}", shopId);
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy thông tin doanh thu" });
            }
        }

        /// <summary>
        /// Lấy phân tích về hiệu suất sản phẩm
        /// </summary>
        /// <param name="shopId">ID của cửa hàng</param>
        /// <returns>Phân tích hiệu suất sản phẩm</returns>
        [HttpGet("{shopId}/analytics/products")]
        public async Task<ActionResult<ProductPerformanceDto>> GetProductPerformance(long shopId)
        {
            try
            {
                var analytics = await _chatbotService.GetProductPerformanceAsync(shopId);
                return Ok(analytics);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid shop ID: {ShopId}", shopId);
                return NotFound(new { message = "Không tìm thấy cửa hàng" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product performance analytics for shop {ShopId}", shopId);
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy thông tin sản phẩm" });
            }
        }

        /// <summary>
        /// Lấy danh sách câu hỏi mẫu cho chatbot
        /// </summary>
        /// <returns>Danh sách câu hỏi mẫu</returns>
        [HttpGet("sample-questions")]
        public ActionResult<object> GetSampleQuestions()
        {
            var sampleQuestions = new
            {
                Revenue = new[]
                {
                    "Doanh thu hôm nay của cửa hàng là bao nhiêu?",
                    "Doanh thu tuần này so với tuần trước như thế nào?",
                    "Doanh thu tháng này tăng hay giảm so với tháng trước?",
                    "Trung bình mỗi đơn hàng có giá trị bao nhiêu?",
                    "Phương thức thanh toán nào được sử dụng nhiều nhất?"
                },
                Customer = new[]
                {
                    "Bao nhiêu % khách hàng đã tạo tài khoản thành viên?",
                    "Khách hàng chủ yếu của cửa hàng là ai?",
                    "Có bao nhiêu khách hàng mới trong tháng này?",
                    "Khách hàng nào chi tiêu nhiều nhất?",
                    "Phân bố khách hàng theo giới tính như thế nào?"
                },
                Inventory = new[]
                {
                    "Tình hình tồn kho hiện tại ra sao?",
                    "Sản phẩm nào sắp hết hàng cần nhập thêm?",
                    "Giá trị tồn kho hiện tại là bao nhiêu?",
                    "Sản phẩm nào đã hết hàng?",
                    "Danh mục nào có nhiều sản phẩm nhất?"
                },
                Product = new[]
                {
                    "Sản phẩm nào bán chạy nhất?",
                    "Sản phẩm nào bán chậm cần chú ý?",
                    "Sản phẩm nào có lợi nhuận cao nhất?",
                    "Những sản phẩm nào cần được chú ý?",
                    "Hiệu suất bán hàng theo từng danh mục?"
                },
                General = new[]
                {
                    "Tổng quan về tình hình kinh doanh của cửa hàng?",
                    "Cửa hàng có bao nhiêu sản phẩm và khách hàng?",
                    "Thống kê tổng quan về cửa hàng",
                    "Báo cáo tình hình kinh doanh hiện tại"
                }
            };

            return Ok(sampleQuestions);
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        /// <returns>Status của chatbot service</returns>
        [HttpGet("health")]
        public ActionResult<object> HealthCheck()
        {
            return Ok(new { 
                status = "healthy", 
                timestamp = DateTime.UtcNow,
                service = "ChatbotService"
            });
        }
    }
}
