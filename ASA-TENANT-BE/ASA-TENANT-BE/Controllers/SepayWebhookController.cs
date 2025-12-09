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
using ASA_TENANT_REPO.Models;
using ASA_TENANT_SERVICE.Enums;
using ASA_TENANT_SERVICE.Interface;
using Microsoft.EntityFrameworkCore;

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
        private readonly OrderRepo _orderRepo;
        private readonly ProductRepo _productRepo;
        private readonly ProductUnitRepo _productUnitRepo;
        private readonly UnitRepo _unitRepo;
        private readonly UserRepo _userRepo;
        private readonly IOrderDetailService _orderDetailService;
        private readonly IInventoryTransactionService _inventoryTransactionService;
        private readonly INotificationService _notificationService;
        private readonly IFcmService _fcmService;
        private readonly IRealtimeNotifier _realtimeNotifier;

        // Giả lập storage để tránh xử lý trùng (thay bằng DB thực tế)
        private static readonly HashSet<string> ProcessedTransactions = new();

        public SepayWebhookController(
            ILogger<SepayWebhookController> logger, 
            IConfiguration config,
            IHubContext<NotificationHub> hubContext,
            ITransactionService transactionService,
            IOrderService orderService,
            ShopRepo shopRepo,
            OrderRepo orderRepo,
            ProductRepo productRepo,
            ProductUnitRepo productUnitRepo,
            UnitRepo unitRepo,
            UserRepo userRepo,
            IOrderDetailService orderDetailService,
            IInventoryTransactionService inventoryTransactionService,
            INotificationService notificationService,
            IFcmService fcmService,
            IRealtimeNotifier realtimeNotifier)
        {
            _logger = logger;
            _config = config;
            _hubContext = hubContext;
            _transactionService = transactionService;
            _orderService = orderService;
            _shopRepo = shopRepo;
            _orderRepo = orderRepo;
            _productRepo = productRepo;
            _productUnitRepo = productUnitRepo;
            _unitRepo = unitRepo;
            _userRepo = userRepo;
            _orderDetailService = orderDetailService;
            _inventoryTransactionService = inventoryTransactionService;
            _notificationService = notificationService;
            _fcmService = fcmService;
            _realtimeNotifier = realtimeNotifier;
        }

        // Model map từ payload SePay (cập nhật theo tài liệu/payload mẫu)
        public class SepayWebhookPayload
        {
            public long id { get; set; }                   // ID giao dịch trên SePay
            public string gateway { get; set; }            // Brand name ngân hàng
            public string transactionDate { get; set; }    // Thời gian giao dịch phía ngân hàng
            public string accountNumber { get; set; }      // Số tài khoản ngân hàng
            public string? code { get; set; }               // Mã code thanh toán (có thể null)
            public string content { get; set; }            // Nội dung chuyển khoản
            public string transferType { get; set; }       // in/out
            public long transferAmount { get; set; }       // Số tiền giao dịch
            public long? accumulated { get; set; }         // Số dư lũy kế (nếu có)
            public string? subAccount { get; set; }         // Tài khoản phụ (nếu có)
            public string referenceCode { get; set; }      // Mã tham chiếu (SePay nhận diện)
            public string description { get; set; }        // Toàn bộ nội dung tin nhắn sms (nếu có)
        }

        [HttpGet("vietqr")]
        public async Task<IActionResult> GenerateVietQr([FromQuery] long orderId)
        {
            try
            {
                // Lấy order
                var orderResult = await _orderService.GetByIdAsync(orderId);
                if (!orderResult.Success || orderResult.Data == null)
                {
                    return NotFound(new { success = false, message = "Order not found" });
                }
                var order = orderResult.Data;

                // Lấy Shop theo ShopId từ Order
                if (order.ShopId == null)
                {
                    return BadRequest(new { success = false, message = "Order missing ShopId" });
                }

                var shops = await _shopRepo.GetAllAsync();
                var shop = shops.FirstOrDefault(s => s.ShopId == order.ShopId);
                if (shop == null)
                {
                    return NotFound(new { success = false, message = "Shop not found" });
                }
                if (shop.Status != 1)
                {
                    return BadRequest(new { success = false, message = "Shop is not active" });
                }

                // Tính số tiền cần thanh toán
                var total = order.FinalPrice ?? 0m;
                if (total <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid order amount" });
                }

                // Ưu tiên sử dụng thông tin ngân hàng từ Shop nếu có
                string baseUrl = null;

                var shopBankCode = shop.BankCode;
                var shopBankAccount = shop.BankNum;
                var shopBankName = shop.BankName;

                if (!string.IsNullOrWhiteSpace(shopBankCode) && !string.IsNullOrWhiteSpace(shopBankAccount))
                {
                    baseUrl = $"https://img.vietqr.io/image/{shopBankCode.Trim()}-{shopBankAccount.Trim()}-compact2.png";
                }

                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    return BadRequest(new { success = false, message = "Missing Shop bank info (BankCode/BankAccount)." });
                }

                // Tạo query theo tài liệu VietQR: amount, addInfo, accountName
                var delimiter = baseUrl.Contains('?') ? "&" : "?";
                var amount = (long)decimal.Round(total, 0, MidpointRounding.AwayFromZero);
                var addInfo = Uri.EscapeDataString($"{order.OrderId}-SEVQR");
                var accName = Uri.EscapeDataString(!string.IsNullOrWhiteSpace(shopBankName) ? shopBankName : (shop.ShopName ?? ""));
                var qrUrl = $"{baseUrl}{delimiter}amount={amount}&addInfo={addInfo}&accountName={accName}";

                return Ok(new { success = true, url = qrUrl, orderId = order.OrderId, amount = amount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi tạo VietQR cho order {OrderId}", orderId);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
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
                var shops = await _shopRepo.GetAllAsync();
                var shop = shops.FirstOrDefault(s => s.SepayApiKey == apiKey);
                
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
                if (payload.id <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid id" });
                }

                if (payload.transferAmount <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid transfer amount" });
                }

                // ✅ Idempotency check (tránh xử lý trùng)
                if (ProcessedTransactions.Contains(payload.id.ToString()))
                {
                    return Ok(new { success = true, info = "already_processed" });
                }

                _logger.LogInformation("Nhận webhook từ SePay cho Shop {ShopId} ({ShopName}): {@Payload}", 
                    shop.ShopId, shop.ShopName, payload);

                // Tìm Order theo content (ưu tiên): lấy số trước "SEVQR" làm orderId
                OrderResponse order = null;
                if (!string.IsNullOrWhiteSpace(payload.content))
                {
                    try
                    {
                        var trimmedContent = payload.content.Trim();
                        var sevqrIndex = trimmedContent.IndexOf("SEVQR", StringComparison.OrdinalIgnoreCase);
                        if (sevqrIndex > 0)
                        {
                            var beforeSevqr = trimmedContent.Substring(0, sevqrIndex).TrimEnd('-');
                            var lastDashIndex = beforeSevqr.LastIndexOf('-');
                            var orderIdSegment = lastDashIndex >= 0 ? beforeSevqr.Substring(lastDashIndex + 1) : beforeSevqr;
                            if (long.TryParse(orderIdSegment, out long contentOrderId))
                            {
                                var orderResult = await _orderService.GetByIdAsync(contentOrderId);
                                if (orderResult.Success)
                                {
                                    order = orderResult.Data;
                                }
                            }
                        }
                    }
                    catch {}
                }

                // Nếu chưa tìm thấy, thử theo referenceCode
                if (order == null && !string.IsNullOrEmpty(payload.referenceCode))
                {
                    if (long.TryParse(payload.referenceCode, out long refOrderId))
                    {
                        var orderResult = await _orderService.GetByIdAsync(refOrderId);
                        if (orderResult.Success) order = orderResult.Data;
                    }
                    if (order == null)
                    {
                        var orderResult = await _orderService.GetByNoteAsync(payload.referenceCode);
                        if (orderResult.Success) order = orderResult.Data;
                    }
                }

                if (order == null)
                {
                    _logger.LogWarning("Không tìm thấy Order với referenceCode: {ReferenceCode}", 
                        payload.referenceCode);
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
                // Status: 0 = Chờ thanh toán, 1 = Đã thanh toán, 2 = Đã hủy
                if (order.Status == 1) // 1 = đã thanh toán
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
                    AppTransId = payload.id.ToString(),
                    ZpTransId = payload.id.ToString(), // SePay transaction ID
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

                // Cập nhật trạng thái Order thành "đã thanh toán" (status = 1)
                // Status: 0 = Chờ thanh toán, 1 = Đã thanh toán, 2 = Đã hủy
                var updateStatusResult = await _orderService.UpdateStatusAsync(order.OrderId, 1);
                if (!updateStatusResult.Success)
                {
                    _logger.LogWarning("Không thể cập nhật status Order {OrderId}: {Message}", 
                        order.OrderId, updateStatusResult.Message);
                }
                else
                {
                    // Thực hiện các bước finalize còn lại cho BankTransfer: phân bổ discount/profit, tạo inventory, trừ kho, notify
                    try
                    {
                        var existing = await _orderRepo.GetFiltered(new Order { OrderId = order.OrderId })
                            .Include(o => o.OrderDetails)
                            .FirstOrDefaultAsync();
                        if (existing != null && existing.OrderDetails != null && existing.OrderDetails.Any())
                        {
                            decimal totalBase = existing.OrderDetails.Sum(d => d.BasePrice);
                            var discountTotal = existing.TotalDiscount ?? 0m;
                            var discountRatio = totalBase > 0 ? (discountTotal / totalBase) : 0m;

                            foreach (var d in existing.OrderDetails)
                            {
                                var basePrice = d.BasePrice;
                                if (basePrice > 0)
                                {
                                    var itemDiscount = basePrice * discountRatio;
                                    var finalPrice = basePrice - itemDiscount;

                                    // Tính profit
                                    var product = await _productRepo.GetByIdAsync(d.ProductId);
                                    var cost = product?.Cost ?? 0m;
                                    var quantity = d.Quantity;
                                    decimal conversionFactor = 1m;
                                    if (d.ProductUnitId > 0)
                                    {
                                        var puForCost = await _productUnitRepo.GetByIdAsync(d.ProductUnitId);
                                        if (puForCost?.ConversionFactor != null && puForCost.ConversionFactor.Value > 0)
                                            conversionFactor = puForCost.ConversionFactor.Value;
                                    }
                                    var profit = finalPrice - (cost * conversionFactor * quantity);

                                    await _orderDetailService.UpdateOrderDetailPricingAsync(d.OrderDetailId, itemDiscount, finalPrice, profit);
                                }

                                // Tạo InventoryTransaction cho mỗi chi tiết
                                if (d.ProductUnitId > 0 && d.ProductId > 0)
                                {
                                    int qtyToDeduct = d.Quantity;
                                    var productUnit = await _productUnitRepo.GetByIdAsync(d.ProductUnitId);
                                    if (productUnit?.ConversionFactor != null && productUnit.ConversionFactor.Value > 0)
                                        qtyToDeduct = (int)(qtyToDeduct * productUnit.ConversionFactor.Value);

                                    await _inventoryTransactionService.CreateAsync(new InventoryTransactionRequest
                                    {
                                        Type = (int)InventoryTransactionType.Sale,
                                        ProductId = d.ProductId,
                                        OrderId = existing.OrderId,
                                        UnitId = productUnit?.UnitId ?? 0L,
                                        Quantity = qtyToDeduct,
                                        Price = 0,
                                        ShopId = existing.ShopId ?? 0
                                    });

                                    // Trừ kho
                                    var productToDeduct = await _productRepo.GetByIdAsync(d.ProductId);
                                    if (productToDeduct?.Quantity != null)
                                    {
                                        productToDeduct.Quantity = Math.Max(0, (productToDeduct.Quantity.Value - qtyToDeduct));
                                        await _productRepo.UpdateAsync(productToDeduct);

                                        // Low stock notify
                                        var threshold = productToDeduct.IsLow ?? 0;
                                        var currentQty = productToDeduct.Quantity ?? 0;
                                        if (currentQty <= threshold && !(productToDeduct.IsLowStockNotified ?? false))
                                        {
                                            var title = "Cảnh báo sắp hết hàng";
                                            string unitName = string.Empty;
                                            if (productToDeduct.UnitIdFk.HasValue)
                                            {
                                                var unit = await _unitRepo.GetByIdAsync(productToDeduct.UnitIdFk.Value);
                                                unitName = unit?.Name ?? string.Empty;
                                            }
                                            var content = $"Sản phẩm {productToDeduct.ProductName} chỉ còn {currentQty} {unitName} (Mức cảnh báo: {threshold}).";

                                            productToDeduct.IsLowStockNotified = true;
                                            await _productRepo.UpdateAsync(productToDeduct);

                                            var shopUsers = await _userRepo.GetFiltered(new User { ShopId = existing.ShopId, Status = 1 })
                                                .Select(u => u.UserId)
                                                .ToListAsync();
                                            foreach (var userId in shopUsers)
                                            {
                                                await _notificationService.CreateAsync(new NotificationRequest
                                                {
                                                    ShopId = existing.ShopId,
                                                    UserId = userId,
                                                    Title = title,
                                                    Content = content,
                                                    Type = (short)NotificationType.Warning,
                                                    IsRead = false,
                                                    CreatedAt = DateTime.UtcNow
                                                });
                                            }

                                            if (existing.ShopId.HasValue)
                                            {
                                                var payloadLow = new
                                                {
                                                    type = (short)NotificationType.Warning,
                                                    shopId = existing.ShopId,
                                                    productId = productToDeduct.ProductId,
                                                    productName = productToDeduct.ProductName,
                                                    currentQuantity = currentQty,
                                                    threshold,
                                                    title,
                                                    content,
                                                    createdAt = DateTime.UtcNow
                                                };
                                                await _realtimeNotifier.EmitLowStockAlertToShop(existing.ShopId.Value, payloadLow);
                                                var usersInShop = await _userRepo.GetFiltered(new User { ShopId = existing.ShopId })
                                                    .Select(u => u.UserId)
                                                    .ToListAsync();
                                                if (usersInShop.Count > 0)
                                                {
                                                    await _fcmService.SendNotificationToManyUsersAsync(usersInShop, title, content);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Finalize logic in SePay callback failed for Order {OrderId}", order.OrderId);
                    }
                }
                

                ProcessedTransactions.Add(payload.id.ToString());

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
