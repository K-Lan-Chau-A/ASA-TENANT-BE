using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ASA_TENANT_BE.Hubs;
using ASA_TENANT_SERVICE.Interface;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Common;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_REPO.Repository;

namespace ASA_TENANT_BE.Controllers
{
    [ApiController]
    [Route("api/sepay")]
    public class SepayWebhookController : ControllerBase
    {
        private readonly ILogger<SepayWebhookController> _logger;
        private readonly IConfiguration _config;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ITransactionService _transactionService;
        private readonly IOrderService _orderService;
        private readonly ShopRepo _shopRepo;

        // Giả lập storage để tránh xử lý trùng (thay bằng DB thực tế)
        private static readonly HashSet<string> ProcessedTransactions = new();

        public SepayWebhookController(
            ILogger<SepayWebhookController> logger, 
            IConfiguration config,
            IHubContext<NotificationHub> hubContext,
            ITransactionService transactionService,
            IOrderService orderService,
            ShopRepo shopRepo)
        {
            _logger = logger;
            _config = config;
            _hubContext = hubContext;
            _transactionService = transactionService;
            _orderService = orderService;
            _shopRepo = shopRepo;
        }

        // Model map từ payload SePay (chỉ demo một số field, bạn bổ sung thêm nếu cần)
        public class SepayWebhookPayload
        {
            public string id { get; set; }           // ID giao dịch
            public string code { get; set; }         // Mã code đơn hàng
            public string referenceCode { get; set; }// Mã tham chiếu
            public string transferType { get; set; } // Loại giao dịch (in/out)
            public decimal transferAmount { get; set; }
            public string transactionDate { get; set; }
            public string content { get; set; }
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> HandleWebhook([FromBody] SepayWebhookPayload payload)
        {
            try
            {
                // ✅ Kiểm tra header Authorization
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();

                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Apikey "))
                {
                    return Unauthorized(new { success = false, message = "Missing Apikey" });
                }

                var apiKey = authHeader.Replace("Apikey ", "").Trim();
                
                // Tìm Shop theo SepayApiKey
                var shop = await _shopRepo.GetAllAsync()
                    .FirstOrDefaultAsync(s => s.SepayApiKey == apiKey);
                
                if (shop == null)
                {
                    _logger.LogWarning("Invalid API key: {ApiKey}", apiKey);
                    return Unauthorized(new { success = false, message = "Invalid Apikey" });
                }

                if (shop.Status != 1) // Giả sử status = 1 là active
                {
                    _logger.LogWarning("Shop {ShopId} is not active", shop.ShopId);
                    return Unauthorized(new { success = false, message = "Shop is not active" });
                }

                // ✅ Đảm bảo payload hợp lệ
                if (string.IsNullOrEmpty(payload.id))
                {
                    return BadRequest(new { success = false, message = "Missing id" });
                }

                if (payload.transferAmount <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid transfer amount" });
                }

                // ✅ Idempotency check (tránh xử lý trùng)
                if (ProcessedTransactions.Contains(payload.id))
                {
                    return Ok(new { success = true, info = "already_processed" });
                }

                _logger.LogInformation("Nhận webhook từ SePay cho Shop {ShopId} ({ShopName}): {@Payload}", 
                    shop.ShopId, shop.ShopName, payload);

                // Tìm Order theo referenceCode hoặc code
                OrderResponse order = null;
                
                // Ưu tiên tìm theo referenceCode trước
                if (!string.IsNullOrEmpty(payload.referenceCode))
                {
                    if (long.TryParse(payload.referenceCode, out long orderId))
                    {
                        // Nếu referenceCode là số, tìm theo OrderId
                        var orderResult = await _orderService.GetByIdAsync(orderId);
                        if (orderResult.Success)
                        {
                            order = orderResult.Data;
                        }
                    }
                    else
                    {
                        // Nếu referenceCode không phải số, tìm theo Note
                        var orderResult = await _orderService.GetByNoteAsync(payload.referenceCode);
                        if (orderResult.Success)
                        {
                            order = orderResult.Data;
                        }
                    }
                }
                
                // Nếu chưa tìm thấy và có code, thử tìm theo code
                if (order == null && !string.IsNullOrEmpty(payload.code))
                {
                    if (long.TryParse(payload.code, out long orderId))
                    {
                        var orderResult = await _orderService.GetByIdAsync(orderId);
                        if (orderResult.Success)
                        {
                            order = orderResult.Data;
                        }
                    }
                    else
                    {
                        var orderResult = await _orderService.GetByNoteAsync(payload.code);
                        if (orderResult.Success)
                        {
                            order = orderResult.Data;
                        }
                    }
                }

                if (order == null)
                {
                    _logger.LogWarning("Không tìm thấy Order với referenceCode: {ReferenceCode} hoặc code: {Code}", 
                        payload.referenceCode, payload.code);
                    return BadRequest(new { success = false, message = "Order not found" });
                }

                // Kiểm tra Order thuộc về đúng Shop
                if (order.ShopId == null || order.ShopId != shop.ShopId)
                {
                    _logger.LogWarning("Order {OrderId} không thuộc về Shop {ShopId}. Order thuộc Shop {OrderShopId}", 
                        order.OrderId, shop.ShopId, order.ShopId);
                    return BadRequest(new { success = false, message = "Order does not belong to this shop" });
                }

                // Kiểm tra Order đã được thanh toán chưa
                if (order.Status == 2) // 2 = đã thanh toán
                {
                    _logger.LogWarning("Order {OrderId} đã được thanh toán trước đó", order.OrderId);
                    return Ok(new { success = true, info = "order_already_paid" });
                }

                // Tạo Transaction record
                var transactionRequest = new TransactionRequest
                {
                    OrderId = order.OrderId,
                    UserId = order.CustomerId, // Sử dụng CustomerId thay vì UserId
                    PaymentStatus = "PAID",
                    AppTransId = payload.id,
                    ZpTransId = payload.id, // SePay transaction ID
                    ReturnCode = 0, // Success
                    ReturnMessage = "Payment successful via SePay",
                    CreatedAt = DateTime.UtcNow
                };

                var transactionResult = await _transactionService.CreateAsync(transactionRequest);

                if (!transactionResult.Success)
                {
                    _logger.LogError("Lỗi tạo Transaction: {Message}", transactionResult.Message);
                    return StatusCode(500, new { success = false, message = "Failed to create transaction" });
                }

                // Cập nhật trạng thái Order thành "đã thanh toán" (status = 2)
                var updateStatusResult = await _orderService.UpdateStatusAsync(order.OrderId, 2);
                if (!updateStatusResult.Success)
                {
                    _logger.LogWarning("Không thể cập nhật status Order {OrderId}: {Message}", 
                        order.OrderId, updateStatusResult.Message);
                }

                ProcessedTransactions.Add(payload.id);

                // Gửi thông báo real-time qua SignalR theo ShopId
                var shopId = shop.ShopId;
                
                // Gửi thông báo đến Customer
                await _hubContext.Clients.Group($"User_{order.CustomerId}")
                    .SendAsync("ReceiveNotification", new
                    {
                        Message = $"Thanh toán thành công cho đơn hàng #{order.OrderId}",
                        Data = new
                        {
                            OrderId = order.OrderId,
                            ShopId = shopId,
                            Amount = payload.transferAmount,
                            Status = "PAID",
                            TransactionId = transactionResult.Data?.TransactionId
                        },
                        Timestamp = DateTime.UtcNow,
                        Type = "PaymentSuccess"
                    });

                // Gửi thông báo đến Shop cụ thể
                await _hubContext.Clients.Group($"Shop_{shopId}")
                    .SendAsync("ReceiveNotification", new
                    {
                        Message = $"Đơn hàng #{order.OrderId} đã được thanh toán thành công",
                        Data = new
                        {
                            OrderId = order.OrderId,
                            ShopId = shopId,
                            CustomerId = order.CustomerId,
                            Amount = payload.transferAmount,
                            Status = "PAID",
                            TransactionId = transactionResult.Data?.TransactionId
                        },
                        Timestamp = DateTime.UtcNow,
                        Type = "PaymentSuccess"
                    });

                // Gửi thông báo đến Admin (tất cả shops)
                await _hubContext.Clients.Group("Admin")
                    .SendAsync("ReceiveNotification", new
                    {
                        Message = $"Đơn hàng #{order.OrderId} tại Shop {shopId} đã được thanh toán thành công",
                        Data = new
                        {
                            OrderId = order.OrderId,
                            ShopId = shopId,
                            CustomerId = order.CustomerId,
                            Amount = payload.transferAmount,
                            Status = "PAID",
                            TransactionId = transactionResult.Data?.TransactionId
                        },
                        Timestamp = DateTime.UtcNow,
                        Type = "PaymentSuccess"
                    });

                _logger.LogInformation("Xử lý webhook SePay thành công cho Order {OrderId} tại Shop {ShopId}, Transaction {TransactionId}, Amount: {Amount}", 
                    order.OrderId, shop.ShopId, transactionResult.Data?.TransactionId, payload.transferAmount);

                // ✅ Trả về thành công
                return Ok(new { success = true, transactionId = transactionResult.Data?.TransactionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xử lý webhook SePay");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
    }
}
