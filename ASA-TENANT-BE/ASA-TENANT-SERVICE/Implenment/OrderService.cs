using ASA_TENANT_REPO.Models;
using ASA_TENANT_REPO.Repository;
using ASA_TENANT_REPO.Models;
using ASA_TENANT_REPO.DBContext;
using ASA_TENANT_SERVICE.DTOs.Common;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Interface;
using AutoMapper;
using ASA_TENANT_SERVICE.Enums;
using Microsoft.EntityFrameworkCore;
using ASA_TENANT_REPO.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Implenment
{
    public class OrderService : IOrderService
    {
        private readonly OrderRepo _orderRepo;
        private readonly OrderDetailRepo _orderDetailRepo;
        private readonly IOrderDetailService _orderDetailService;
        private readonly IInventoryTransactionService _inventoryTransactionService;
        private readonly ProductUnitRepo _productUnitRepo;
        private readonly IMapper _mapper;
        private readonly VoucherRepo _voucherRepo;
        private readonly ProductRepo _productRepo;
        private readonly INotificationService _notificationService;
        private readonly IFcmService _fcmService;
        private readonly IRealtimeNotifier _realtimeNotifier;
        private readonly UnitRepo _unitRepo;
        private readonly UserRepo _userRepo;
        private readonly ICustomerService _customerService;
        private readonly CustomerRepo _customerRepo;
        private readonly ShopRepo _shopRepo;
        private readonly ASATENANTDBContext _context;
        private readonly IEmailService _emailService;
        private readonly PromotionProductRepo _promotionProductRepo;
        public OrderService(OrderRepo orderRepo, IOrderDetailService orderDetailService, IInventoryTransactionService inventoryTransactionService, ProductUnitRepo productUnitRepo, IMapper mapper, VoucherRepo voucherRepo, ProductRepo productRepo, INotificationService notificationService, IFcmService fcmService, IRealtimeNotifier realtimeNotifier, UserRepo userRepo, UnitRepo unitRepo, ICustomerService customerService, CustomerRepo customerRepo, ShopRepo shopRepo, ASATENANTDBContext context, IEmailService emailService, OrderDetailRepo orderDetailRepo, PromotionProductRepo promotionProductRepo)
        {
            _orderRepo = orderRepo;
            _orderDetailService = orderDetailService;
            _inventoryTransactionService = inventoryTransactionService;
            _productUnitRepo = productUnitRepo;
            _mapper = mapper;
            _voucherRepo = voucherRepo;
            _productRepo = productRepo;
            _notificationService = notificationService;
            _fcmService = fcmService;
            _realtimeNotifier = realtimeNotifier;
            _userRepo = userRepo;
            _unitRepo = unitRepo;
            _customerService = customerService;
            _customerRepo = customerRepo;
            _shopRepo = shopRepo;
            _context = context;
            _emailService = emailService;
            _orderDetailRepo = orderDetailRepo;
            _promotionProductRepo = promotionProductRepo;
        }

        public async Task<ApiResponse<OrderResponse>> GetByIdAsync(long id)
        {
            try
            {
                var order = await _orderRepo.GetByIdAsync(id);
                if (order == null)
                {
                    return new ApiResponse<OrderResponse>
                    {
                        Success = false,
                        Message = "Order not found",
                        Data = null
                    };
                }

                var response = _mapper.Map<OrderResponse>(order);
                return new ApiResponse<OrderResponse>
                {
                    Success = true,
                    Message = "Order found",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<OrderResponse>> GetByNoteAsync(string note)
        {
            try
            {
                var orders = await _orderRepo.GetAllAsync();
                var order = orders.FirstOrDefault(o => o.Note == note);

                if (order == null)
                {
                    return new ApiResponse<OrderResponse>
                    {
                        Success = false,
                        Message = "Order not found",
                        Data = null
                    };
                }

                var response = _mapper.Map<OrderResponse>(order);
                return new ApiResponse<OrderResponse>
                {
                    Success = true,
                    Message = "Order found",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<OrderResponse>> CreateAsync(OrderRequest request)
        {
            try
            {
                // Tạo Order trước
                var entity = _mapper.Map<Order>(request);
                
                // Set Datetime to current time (UTC for PostgreSQL compatibility)
                entity.Datetime = DateTime.UtcNow;
                
                // Set CreatedAt to current time for expiration tracking
                entity.CreatedAt = DateTime.UtcNow;
                
                // Xác định trạng thái thanh toán theo yêu cầu: chỉ ghi nhận kết quả khi
                // - status = 1 (Paid) hoặc
                // - paymentMethod = Cash
                // Status: 0 = Pending, 1 = Paid, 2 = Cancelled
                var isCash = request.PaymentMethod == PaymentMethodEnum.Cash;
                var isBankTransfer = request.PaymentMethod == PaymentMethodEnum.BankTransfer;
                var isPaidInput = request.Status == (short)OrderStatus.Paid;
                var shouldFinalize = (isCash || isPaidInput) && !isBankTransfer;
                if (isBankTransfer)
                {
                    // Bank transfer orders are not finalized at creation; force Pending
                    entity.Status = (short)OrderStatus.Pending;
                }
                else
                {
                    entity.Status = shouldFinalize ? (short)OrderStatus.Paid : (request.Status ?? (short)OrderStatus.Pending);
                }
                
                // Xử lý voucherId: nếu = 0 hoặc không hợp lệ thì set null
                if (request.VoucherId.HasValue && request.VoucherId.Value <= 0)
                {
                    entity.VoucherId = null;
                }
                // Xử lý voucherId: nếu = 0 hoặc không hợp lệ thì set null
                if (request.CustomerId.HasValue && request.CustomerId.Value <= 0)
                {
                    entity.CustomerId = null;
                }
                // Khởi tạo các trường pricing
                entity.TotalPrice = 0m; // Tổng tiền sản phẩm (chưa giảm giá)
                entity.TotalDiscount = 0m; // Tổng tiền giảm giá
                entity.FinalPrice = 0m; // Tiền cuối cùng phải trả
                
                var affected = await _orderRepo.CreateAsync(entity);
                if (affected <= 0)
                {
                    return new ApiResponse<OrderResponse>
                    {
                        Success = false,
                        Message = "Create Order failed",
                        Data = null
                    };
                }

                // Tính toán các loại discount
                var discountAmount = 0m;
                var voucherAmount = 0m;
                var rankBenefitAmount = 0m;
                var discountNotes = new List<string>();
                
                // Không cần tính rank benefit ở đây, sẽ tính ở phần sau
                
                // Lưu discount rate từ request (phần trăm) và chuẩn hóa về [0, 100]
                var normalizedDiscount = request.Discount ?? 0m;
                if (normalizedDiscount < 0m) normalizedDiscount = 0m;
                if (normalizedDiscount > 100m) normalizedDiscount = 100m;
                entity.Discount = normalizedDiscount;

                // Chỉ tạo OrderDetails, tính toán giá, trừ kho, ghi chú, cộng spent... khi shouldFinalize = true
                var createdOrderDetails = new List<OrderDetailResponse>();
                decimal totalProductPrice = 0m; // Tổng tiền sản phẩm (chưa giảm giá)
                decimal grossRevenueTotal = 0m; // Tổng giá bán gốc (không khuyến mãi, không chiết khấu)
                if (shouldFinalize && request.OrderDetails != null && request.OrderDetails.Any())
                {
                    // BƯỚC 1: KIỂM TRA TẤT CẢ SẢN PHẨM TRƯỚC KHI TẠO ORDER
                    var stockValidationResults = new List<(Product Product, int QuantityToDeduct)>();
                    
                    foreach (var orderDetailRequest in request.OrderDetails)
                    {
                        if (orderDetailRequest.ProductId.HasValue)
                        {
                            var product = await _productRepo.GetByIdAsync(orderDetailRequest.ProductId.Value);
                            if (product != null && product.Quantity.HasValue)
                            {
                                int quantityToDeduct = orderDetailRequest.Quantity ?? 0;
                                
                                // Áp dụng conversion factor nếu có ProductUnit
                                if (orderDetailRequest.ProductUnitId.HasValue && orderDetailRequest.ProductUnitId.Value > 0)
                                {
                                    var productUnit = await _productUnitRepo.GetByIdAsync(orderDetailRequest.ProductUnitId.Value);
                                    if (productUnit != null && productUnit.ConversionFactor.HasValue && productUnit.ConversionFactor.Value > 0)
                                    {
                                        quantityToDeduct = (int)(quantityToDeduct * productUnit.ConversionFactor.Value);
                                    }
                                }
                                
                                // Kiểm tra tồn kho
                                if (product.Quantity.Value < quantityToDeduct)
                                {
                                    return new ApiResponse<OrderResponse>
                                    {
                                        Success = false,
                                        Message = $"Insufficient stock for product '{product.ProductName}'. Required={quantityToDeduct}, Available={product.Quantity.Value}",
                                        Data = null
                                    };
                                }
                                
                                stockValidationResults.Add((product, quantityToDeduct));
                            }
                            else
                            {
                                return new ApiResponse<OrderResponse>
                                {
                                    Success = false,
                                    Message = $"Product with ID {orderDetailRequest.ProductId.Value} not found or has no quantity",
                                    Data = null
                                };
                            }
                        }
                    }
                    
                    // BƯỚC 2: TẤT CẢ SẢN PHẨM ĐỀU ĐỦ, TIẾN HÀNH TẠO ORDER
                    foreach (var orderDetailRequest in request.OrderDetails)
                    {
                        // Tạo OrderDetailRequest mới với OrderId
                        var orderDetailWithOrderId = new OrderDetailRequest
                        {
                            Quantity = orderDetailRequest.Quantity,
                            ProductUnitId = orderDetailRequest.ProductUnitId,
                            ProductId = orderDetailRequest.ProductId
                        };
                        
                        var orderDetailResult = await _orderDetailService.CreateAsync(orderDetailWithOrderId, entity.OrderId);
                        if (orderDetailResult.Success && orderDetailResult.Data != null)
                        {
                            createdOrderDetails.Add(orderDetailResult.Data);

                            // Tạo InventoryTransaction cho OrderDetail này
                            await CreateInventoryTransactionForOrderDetail(orderDetailWithOrderId, entity);
                        }
                        else
                        {
                            return new ApiResponse<OrderResponse>
                            {
                                Success = false,
                                Message = "Failed to create order detail",
                                Data = null
                            };
                        }
                    }
                    
                    // TÍNH GROSS REVENUE (giá gốc) TRƯỚC KHI ÁP DỤNG KHUYẾN MÃI
                    foreach (var orderDetail in createdOrderDetails)
                    {
                        if (orderDetail.ProductId > 0 && orderDetail.Quantity > 0)
                        {
                            decimal originalUnitPrice = 0m;
                            if (orderDetail.ProductUnitId > 0)
                            {
                                var productUnitForOriginal = await _productUnitRepo.GetByIdAsync(orderDetail.ProductUnitId);
                                if (productUnitForOriginal?.Price != null)
                                {
                                    originalUnitPrice = productUnitForOriginal.Price.Value;
                                }
                            }
                            if (originalUnitPrice == 0m)
                            {
                                var productForOriginal = await _productRepo.GetByIdAsync(orderDetail.ProductId);
                                if (productForOriginal?.Price != null)
                                {
                                    originalUnitPrice = productForOriginal.Price.Value;
                                }
                            }
                            if (originalUnitPrice > 0m)
                            {
                                grossRevenueTotal += originalUnitPrice * orderDetail.Quantity;
                            }
                        }
                    }

                    // BƯỚC 2.5: ÁP DỤNG PROMOTION PRICE CHO TỪNG ORDERDETAIL
                    // Tính promotion price và cập nhật BasePrice của từng OrderDetail
                    foreach (var orderDetail in createdOrderDetails)
                    {
                        if (orderDetail.ProductId > 0 && orderDetail.Quantity > 0 && orderDetail.BasePrice > 0)
                        {
                            var originalUnitPrice = orderDetail.BasePrice / orderDetail.Quantity;
                            long? unitIdForPromo = null;
                            if (orderDetail.ProductUnitId > 0)
                            {
                                var pu = await _productUnitRepo.GetByIdAsync(orderDetail.ProductUnitId);
                                unitIdForPromo = pu?.UnitId;
                            }
                            
                            // Tính promotion price
                            var promoPricePerUnit = await CalculatePromotionUnitPriceAsync(orderDetail.ProductId, entity.ShopId, unitIdForPromo, originalUnitPrice);
                            
                            // BasePrice mới = promotion price (nếu có) hoặc price gốc (nếu không có promotion)
                            decimal newBasePrice = orderDetail.BasePrice;
                            if (promoPricePerUnit.HasValue && promoPricePerUnit.Value < originalUnitPrice)
                            {
                                newBasePrice = promoPricePerUnit.Value * orderDetail.Quantity;
                                // Cập nhật BasePrice trong database
                                await _orderDetailService.UpdateBasePriceAsync(orderDetail.OrderDetailId, newBasePrice);
                            }
                            
                            // Cập nhật lại BasePrice trong OrderDetail response để dùng cho tính toán tiếp theo
                            orderDetail.BasePrice = newBasePrice;
                            totalProductPrice += newBasePrice;
                            
                            Console.WriteLine($"OrderDetail {orderDetail.OrderDetailId}: Original Price={originalUnitPrice:N0}, Promotion Price={promoPricePerUnit ?? originalUnitPrice:N0}, BasePrice={newBasePrice:N0} đ");
                        }
                    }
                    
                    // Cập nhật TotalPrice = tổng BasePrice (đã là promotion price rồi)
                    entity.TotalPrice = totalProductPrice;
                    // Set GrossRevenue = tổng giá gốc trước khuyến mãi/chiết khấu
                    entity.GrossRevenue = grossRevenueTotal;
                    
                    // BƯỚC 3: TÍNH TOÁN DISCOUNT VÀ FINAL PRICE
                    // Lưu ý: TotalPrice giờ đã là promotion price rồi, nên các discount tính trên TotalPrice này
                    
                    // 3.1 Tính discount % từ request (áp dụng trên TotalPrice = promotion price)
                    if (entity.Discount.HasValue && entity.Discount.Value > 0)
                    {
                        discountAmount = totalProductPrice * (entity.Discount.Value / 100m);
                        discountNotes.Add($"Giảm {entity.Discount.Value:F2}% từ chiết khấu: {discountAmount:N0} đ");
                    }
                    
                    // 3.2 Tính voucher discount nếu có voucher (áp dụng trên TotalPrice = promotion price)
                    if (request.VoucherId.HasValue && request.VoucherId.Value > 0)
                    {
                        var voucher = await _voucherRepo.GetByIdAsync(request.VoucherId.Value);
                        if (voucher == null)
                        {
                            return new ApiResponse<OrderResponse>
                            {
                                Success = false,
                                Message = "Voucher not found",
                                Data = null
                            };
                        }

                        // Kiểm tra hết hạn
                        if (voucher.Expired.HasValue && voucher.Expired.Value < DateTime.UtcNow)
                        {
                            return new ApiResponse<OrderResponse>
                            {
                                Success = false,
                                Message = "Voucher expired",
                                Data = null
                            };
                        }

                        // Kiểm tra khớp Shop
                        if (voucher.ShopId.HasValue && voucher.ShopId.Value != request.ShopId)
                        {
                            return new ApiResponse<OrderResponse>
                            {
                                Success = false,
                                Message = "Voucher is not valid for this shop",
                                Data = null
                            };
                        }

                        // Tính discount từ voucher (tính trên TotalPrice = promotion price)
                        if (voucher.Type == 1 && voucher.Value.HasValue)
                        {
                            // Trừ số tiền cố định
                            voucherAmount = voucher.Value.Value;
                            discountNotes.Add($"Voucher {voucher.Code}: Giảm tổng đơn {voucherAmount:N0} đ");
                        }
                        else if (voucher.Type == 2 && voucher.Value.HasValue)
                        {
                            // Trừ theo phần trăm (0..100%)
                            var rate = voucher.Value.Value;
                            if (rate < 0m) rate = 0m;
                            if (rate > 100m) rate = 100m;
                            voucherAmount = totalProductPrice * (rate / 100m);
                            discountNotes.Add($"Voucher {voucher.Code}: Giảm {rate:F2}% = {voucherAmount:N0} đ");
                        }
                    }
                    
                    // 3.3 Tính rank benefit discount (áp dụng trên TotalPrice = promotion price)
                    if (request.CustomerId.HasValue)
                    {
                        var customer = await _customerRepo.GetByIdAsync(request.CustomerId.Value);
                        if (customer?.Rank?.Benefit.HasValue == true)
                        {
                            var rankBenefitRate = (decimal)customer.Rank.Benefit.Value;
                            rankBenefitAmount = totalProductPrice * rankBenefitRate;
                            discountNotes.Add($"Chiết khấu khách hàng {customer.Rank.RankName}: Giảm {rankBenefitRate * 100:F2}% = {rankBenefitAmount:N0} đ");
                            
                            Console.WriteLine($"Customer {customer.CustomerId} rank {customer.Rank.RankName} có benefit {rankBenefitRate * 100:F2}%");
                        }
                    }
                    
                    // Tính tổng discount (KHÔNG bao gồm promotion vì TotalPrice đã là promotion price rồi)
                    entity.TotalDiscount = discountAmount + voucherAmount + rankBenefitAmount;
                    
                    // Tính final price
                    entity.FinalPrice = entity.TotalPrice - entity.TotalDiscount;
                    if (entity.FinalPrice < 0) entity.FinalPrice = 0;
                    
                    // Tạo note với thông tin discount
                    if (discountNotes.Any())
                    {
                        entity.Note = string.Join("; ", discountNotes);
                    }
                    
                    Console.WriteLine($"Order {entity.OrderId} Pricing:");
                    Console.WriteLine($"  TotalPrice (Promotion Price): {entity.TotalPrice:N0} đ");
                    Console.WriteLine($"  Discount amount: {discountAmount:N0} đ");
                    Console.WriteLine($"  Voucher amount: {voucherAmount:N0} đ");
                    Console.WriteLine($"  Rank benefit amount: {rankBenefitAmount:N0} đ");
                    Console.WriteLine($"  TotalDiscount: {entity.TotalDiscount:N0} đ");
                    Console.WriteLine($"  FinalPrice: {entity.FinalPrice:N0} đ");
                    
                    // BƯỚC 3.5: PHÂN BỔ DISCOUNT VÀ TÍNH PROFIT CHO TỪNG ORDERDETAIL
                    // TotalPrice đã là promotion price, nên chỉ phân bổ các discount toàn cục (voucher, benefit, discount%)
                    if ((entity.TotalDiscount ?? 0m) > 0 && totalProductPrice > 0)
                    {
                        var discountRatio = totalProductPrice > 0 ? ((entity.TotalDiscount ?? 0m) / totalProductPrice) : 0m;
                        
                        foreach (var orderDetail in createdOrderDetails)
                        {
                            if (orderDetail.BasePrice > 0)
                            {
                                // Giảm giá toàn cục phân bổ theo tỷ lệ (không có promotion discount nữa vì BasePrice đã là promotion price)
                                var itemDiscount = orderDetail.BasePrice * discountRatio;
                                var finalPrice = orderDetail.BasePrice - itemDiscount;
                                
                                // Tính profit = finalPrice - (cost theo đơn vị) * quantity
                                var product = await _productRepo.GetByIdAsync(orderDetail.ProductId);
                                var cost = product?.Cost ?? 0m;
                                var quantity = orderDetail.Quantity;
                                decimal conversionFactor = 1m;
                                if (orderDetail.ProductUnitId > 0)
                                {
                                    var puForCost = await _productUnitRepo.GetByIdAsync(orderDetail.ProductUnitId);
                                    if (puForCost?.ConversionFactor != null && puForCost.ConversionFactor.Value > 0)
                                        conversionFactor = puForCost.ConversionFactor.Value;
                                }
                                var profit = finalPrice - (cost * conversionFactor * quantity);
                                
                                // Cập nhật OrderDetail với discount và profit (DB)
                                await _orderDetailService.UpdateOrderDetailPricingAsync(orderDetail.OrderDetailId, itemDiscount, finalPrice, profit);
                                
                                // Đồng bộ đối tượng trả về
                                orderDetail.DiscountAmount = itemDiscount;
                                orderDetail.FinalPrice = finalPrice;
                                orderDetail.Profit = profit;
                                
                                // Cập nhật InventoryTransaction profit
                                await UpdateInventoryTransactionProfit(orderDetail.OrderDetailId, profit);
                                
                                Console.WriteLine($"OrderDetail {orderDetail.OrderDetailId}: BasePrice={orderDetail.BasePrice:N0}, Discount={itemDiscount:N0}, FinalPrice={finalPrice:N0}, Profit={profit:N0}");
                            }
                        }
                    }
                    
                    // BƯỚC 4: TRỪ TỒN KHO SAU KHI TẤT CẢ ĐÃ THÀNH CÔNG
                    foreach (var (product, quantityToDeduct) in stockValidationResults)
                    {
                        product.Quantity = Math.Max(0, (product.Quantity.Value - quantityToDeduct));
                        
                        // Reset IsLowStockNotified nếu quantity > threshold
                        var threshold = product.IsLow ?? 0;
                        var currentQty = product.Quantity ?? 0;
                        if (currentQty > threshold && (product.IsLowStockNotified ?? false))
                        {
                            product.IsLowStockNotified = false;
                            Console.WriteLine($"Reset IsLowStockNotified = false for product {product.ProductName} (quantity: {currentQty} > threshold: {threshold})");
                        }
                        
                        await _productRepo.UpdateAsync(product);

                        // Low stock check and notify
                        if (currentQty <= threshold)
                        {
                            if (!(product.IsLowStockNotified ?? false))
                            {
                                var title = "Cảnh báo sắp hết hàng";
                                string unitName = string.Empty;
                                if (product.UnitIdFk.HasValue)
                                {
                                    var unit = await _unitRepo.GetByIdAsync(product.UnitIdFk.Value);
                                    unitName = unit?.Name ?? string.Empty;
                                }
                                var content = $"Sản phẩm {product.ProductName} chỉ còn {currentQty} {unitName} (Mức cảnh báo: {threshold}).";

                                // Đặt cờ trước để tránh spam kể cả khi dispatch lỗi
                                product.IsLowStockNotified = true;
                                await _productRepo.UpdateAsync(product);
                                
                                Console.WriteLine($"Set IsLowStockNotified = true for product {product.ProductName} (ID: {product.ProductId})");

                                try
                                {
                                    // Lấy tất cả user trong shop để lưu notification
                                    var shopUsers = await _userRepo.GetFiltered(new User 
                                    { 
                                        ShopId = entity.ShopId, 
                                        Status = 1 
                                    })
                                    .Select(u => u.UserId)
                                    .ToListAsync();

                                    // Lưu notification cho tất cả user trong shop
                                    foreach (var userId in shopUsers)
                                    {
                                        await _notificationService.CreateAsync(new NotificationRequest
                                        {
                                            ShopId = entity.ShopId,
                                            UserId = userId,
                                            Title = title,
                                            Content = content,
                                            Type = (short)NotificationType.Warning,
                                            IsRead = false,
                                            CreatedAt = DateTime.UtcNow
                                        });
                                    }
                                    
                                    Console.WriteLine($"Low stock notification saved for {shopUsers.Count} users in shop {entity.ShopId}");

                                    var payload = new
                                    {
                                        type = (short)NotificationType.Warning,
                                        shopId = entity.ShopId,
                                        productId = product.ProductId,
                                        productName = product.ProductName,
                                        currentQuantity = currentQty,
                                        threshold = threshold,
                                        title = title,
                                        content = content,
                                        createdAt = DateTime.UtcNow
                                    };

                                    // Emit SignalR to shop group
                                    if (entity.ShopId.HasValue)
                                    {
                                        await _realtimeNotifier.EmitLowStockAlertToShop(entity.ShopId.Value, payload);
                                    }

                                    // Send FCM to all users of the shop
                                    if (entity.ShopId.HasValue)
                                    {
                                        var usersInShop = await _userRepo
                                            .GetFiltered(new User { ShopId = entity.ShopId })
                                            .Select(u => u.UserId)
                                            .ToListAsync();
                                        if (usersInShop.Count > 0)
                                        {
                                            await _fcmService.SendNotificationToManyUsersAsync(usersInShop, title, content);
                                        }
                                    }
                                }
                                catch
                                {
                                    // Nuốt lỗi gửi thông báo để không làm hỏng luồng tạo đơn
                                }
                            }
                        }
                    }
                }

                // Nếu là BankTransfer (không finalize), vẫn tạo OrderDetails nhưng không trừ kho/không phân bổ profit
                if (!shouldFinalize && isBankTransfer && request.OrderDetails != null && request.OrderDetails.Any())
                {
                    // BƯỚC 1: KIỂM TRA TỒN KHO (không trừ kho ở nhánh BankTransfer)
                    var validationResults = new List<(Product Product, int QuantityToDeduct)>();
                    foreach (var od in request.OrderDetails)
                    {
                        if (od.ProductId.HasValue)
                        {
                            var product = await _productRepo.GetByIdAsync(od.ProductId.Value);
                            if (product != null && product.Quantity.HasValue)
                            {
                                int qty = od.Quantity ?? 0;
                                if (od.ProductUnitId.HasValue && od.ProductUnitId.Value > 0)
                                {
                                    var productUnit = await _productUnitRepo.GetByIdAsync(od.ProductUnitId.Value);
                                    if (productUnit?.ConversionFactor != null && productUnit.ConversionFactor.Value > 0)
                                        qty = (int)(qty * productUnit.ConversionFactor.Value);
                                }
                                if (product.Quantity.Value < qty)
                                {
                                    return new ApiResponse<OrderResponse>
                                    {
                                        Success = false,
                                        Message = $"Insufficient stock for product '{product.ProductName}'. Required={qty}, Available={product.Quantity.Value}",
                                        Data = null
                                    };
                                }
                                validationResults.Add((product, qty));
                            }
                            else
                            {
                                return new ApiResponse<OrderResponse>
                                {
                                    Success = false,
                                    Message = $"Product with ID {od.ProductId.Value} not found or has no quantity",
                                    Data = null
                                };
                            }
                        }
                    }

                    // BƯỚC 2: TẠO ORDER DETAILS (không tạo InventoryTransaction)
                    foreach (var od in request.OrderDetails)
                    {
                        var odReq = new OrderDetailRequest
                        {
                            Quantity = od.Quantity,
                            ProductUnitId = od.ProductUnitId,
                            ProductId = od.ProductId
                        };
                        var odResult = await _orderDetailService.CreateAsync(odReq, entity.OrderId);
                        if (!odResult.Success || odResult.Data == null)
                        {
                            return new ApiResponse<OrderResponse>
                            {
                                Success = false,
                                Message = "Failed to create order detail",
                                Data = null
                            };
                        }
                        createdOrderDetails.Add(odResult.Data);
                        // Không tạo inventory transaction ở nhánh BankTransfer
                    }

                    // BƯỚC 3: TÍNH GROSS REVENUE + PROMOTION PRICE
                    foreach (var d in createdOrderDetails)
                    {
                        if (d.ProductId > 0 && d.Quantity > 0)
                        {
                            decimal originalUnitPrice = 0m;
                            if (d.ProductUnitId > 0)
                            {
                                var puOriginal = await _productUnitRepo.GetByIdAsync(d.ProductUnitId);
                                if (puOriginal?.Price != null) originalUnitPrice = puOriginal.Price.Value;
                            }
                            if (originalUnitPrice == 0m)
                            {
                                var p = await _productRepo.GetByIdAsync(d.ProductId);
                                if (p?.Price != null) originalUnitPrice = p.Price.Value;
                            }
                            if (originalUnitPrice > 0m)
                            {
                                grossRevenueTotal += originalUnitPrice * d.Quantity;
                            }

                            if (d.BasePrice > 0)
                            {
                                var originalUnit = d.BasePrice / d.Quantity;
                                long? unitIdForPromo = null;
                                if (d.ProductUnitId > 0)
                                {
                                    var pu = await _productUnitRepo.GetByIdAsync(d.ProductUnitId);
                                    unitIdForPromo = pu?.UnitId;
                                }
                                var promoPrice = await CalculatePromotionUnitPriceAsync(d.ProductId, entity.ShopId, unitIdForPromo, originalUnit);
                                decimal newBase = d.BasePrice;
                                if (promoPrice.HasValue && promoPrice.Value < originalUnit)
                                {
                                    newBase = promoPrice.Value * d.Quantity;
                                    await _orderDetailService.UpdateBasePriceAsync(d.OrderDetailId, newBase);
                                }
                                d.BasePrice = newBase;
                                totalProductPrice += newBase;
                            }
                        }
                    }

                    entity.TotalPrice = totalProductPrice;
                    entity.GrossRevenue = grossRevenueTotal;

                    // BƯỚC 4: TÍNH DISCOUNT & FINAL PRICE & NOTE
                    if (entity.Discount.HasValue && entity.Discount.Value > 0)
                    {
                        discountAmount = totalProductPrice * (entity.Discount.Value / 100m);
                        discountNotes.Add($"Giảm {entity.Discount.Value:F2}% từ chiết khấu: {discountAmount:N0} đ");
                    }

                    if (request.VoucherId.HasValue && request.VoucherId.Value > 0)
                    {
                        var voucher = await _voucherRepo.GetByIdAsync(request.VoucherId.Value);
                        if (voucher == null)
                        {
                            return new ApiResponse<OrderResponse> { Success = false, Message = "Voucher not found", Data = null };
                        }
                        if (voucher.Expired.HasValue && voucher.Expired.Value < DateTime.UtcNow)
                        {
                            return new ApiResponse<OrderResponse> { Success = false, Message = "Voucher expired", Data = null };
                        }
                        if (voucher.ShopId.HasValue && voucher.ShopId.Value != request.ShopId)
                        {
                            return new ApiResponse<OrderResponse> { Success = false, Message = "Voucher is not valid for this shop", Data = null };
                        }
                        if (voucher.Type == 1 && voucher.Value.HasValue)
                        {
                            voucherAmount = voucher.Value.Value;
                            discountNotes.Add($"Voucher {voucher.Code}: Giảm tổng đơn {voucherAmount:N0} đ");
                        }
                        else if (voucher.Type == 2 && voucher.Value.HasValue)
                        {
                            var rate = voucher.Value.Value; if (rate < 0m) rate = 0m; if (rate > 100m) rate = 100m;
                            voucherAmount = totalProductPrice * (rate / 100m);
                            discountNotes.Add($"Voucher {voucher.Code}: Giảm {rate:F2}% = {voucherAmount:N0} đ");
                        }
                    }

                    if (request.CustomerId.HasValue)
                    {
                        var customer = await _customerRepo.GetByIdAsync(request.CustomerId.Value);
                        if (customer?.Rank?.Benefit.HasValue == true)
                        {
                            var rankBenefitRate = (decimal)customer.Rank.Benefit.Value;
                            rankBenefitAmount = totalProductPrice * rankBenefitRate;
                            discountNotes.Add($"Chiết khấu khách hàng {customer.Rank.RankName}: Giảm {rankBenefitRate * 100:F2}% = {rankBenefitAmount:N0} đ");
                        }
                    }

                    entity.TotalDiscount = discountAmount + voucherAmount + rankBenefitAmount;
                    entity.FinalPrice = entity.TotalPrice - entity.TotalDiscount;
                    if (entity.FinalPrice < 0) entity.FinalPrice = 0;
                    if (discountNotes.Any()) entity.Note = string.Join("; ", discountNotes);
                }

                // Cập nhật Order với pricing information
                await _orderRepo.UpdateAsync(entity);

                if (shouldFinalize)
                {
                    // Nếu payment method là Cash hoặc status=Paid và có customerId, cập nhật spent và rank của customer
                    if (entity.CustomerId.HasValue && entity.FinalPrice > 0)
                    {
                        Console.WriteLine($"Updating customer {entity.CustomerId.Value} spent with order total: {entity.FinalPrice} (Finalize order)");
                        await _customerService.UpdateCustomerSpentAndRankAsync(entity.CustomerId.Value, entity.FinalPrice ?? 0);
                    }

                    // Gửi email hóa đơn nếu FE yêu cầu và có customer hợp lệ
                    if ((request.IsSendInvoice ?? false) && entity.CustomerId.HasValue)
                    {
                        var customer = await _customerRepo.GetByIdAsync(entity.CustomerId.Value);
                        if (customer != null && !string.IsNullOrWhiteSpace(customer.Email))
                        {
                            await SendOrderConfirmationEmailAsync(entity, createdOrderDetails);
                        }
                    }
                }

                // Clear OrderDetails trong entity trước khi map để tránh AutoMapper map duplicate
                var tempOrderDetails = entity.OrderDetails;
                entity.OrderDetails = null;
                var response = _mapper.Map<OrderResponse>(entity);
                // Restore OrderDetails trong entity (nếu cần cho logic khác)
                entity.OrderDetails = tempOrderDetails;
                // Chỉ dùng createdOrderDetails, không dùng OrderDetails từ entity
                response.OrderDetails = createdOrderDetails ?? new List<OrderDetailResponse>();
                return new ApiResponse<OrderResponse>
                {
                    Success = true,
                    Message = "Create successfully",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }

        private async Task<decimal?> CalculatePromotionUnitPriceAsync(long productId, long? shopId, long? unitId, decimal baseUnitPrice)
        {
            try
            {
                var filter = new PromotionProduct { ProductId = productId };
                if (unitId.HasValue && unitId.Value > 0)
                {
                    filter.UnitId = unitId.Value;
                }
                var promosQuery = _promotionProductRepo.GetFiltered(filter)
                    .Select(pp => pp.Promotion);

                var promos = await promosQuery.ToListAsync();
                if (promos == null || promos.Count == 0)
                    return null;

                var now = DateTime.Now;
                var today = DateOnly.FromDateTime(now);
                var currentTime = TimeOnly.FromDateTime(now);

                decimal currentPrice = baseUnitPrice;
                bool anyApplied = false;

                foreach (var promo in promos
                    .Where(p => p != null)
                    .OrderBy(p => p.Type == (short)PromotionType.Percentage ? 0 : 1))
                {
                    if (promo.Status != (short?)PromotionStatus.Active)
                        continue;

                    if (promo.ShopId != null && shopId != null && promo.ShopId != shopId)
                        continue;

                    if (promo.StartDate != null && today < promo.StartDate.Value)
                        continue;
                    if (promo.EndDate != null && today > promo.EndDate.Value)
                        continue;

                    if (promo.StartTime != null && promo.EndTime != null)
                    {
                        if (currentTime < promo.StartTime.Value || currentTime > promo.EndTime.Value)
                            continue;
                    }
                    else if (promo.StartTime != null && currentTime < promo.StartTime.Value)
                    {
                        continue;
                    }
                    else if (promo.EndTime != null && currentTime > promo.EndTime.Value)
                    {
                        continue;
                    }

                    if (promo.Type == null || promo.Value == null)
                        continue;

                    if (promo.Type == (short)PromotionType.Percentage)
                    {
                        var pct = promo.Value.Value;
                        if (pct < 0) pct = 0;
                        if (pct > 100) pct = 100;
                        currentPrice = currentPrice - (currentPrice * (pct / 100m));
                        anyApplied = true;
                    }
                    else if (promo.Type == (short)PromotionType.Money)
                    {
                        currentPrice = currentPrice - promo.Value.Value;
                        anyApplied = true;
                    }

                    if (currentPrice < 0)
                        currentPrice = 0;
                }

                if (!anyApplied)
                    return null;

                return currentPrice;
            }
            catch
            {
                return null;
            }
        }

        public async Task<ApiResponse<OrderResponse>> UpdateStatusAsync(long id, short status)
        {
            try
            {
                var existing = await _orderRepo.GetByIdAsync(id);
                if (existing == null)
                {
                    return new ApiResponse<OrderResponse>
                    {
                        Success = false,
                        Message = "Order not found",
                        Data = null
                    };
                }

                // Kiểm tra nếu status chuyển từ pending (0) sang paid (1) và có customerId
                var wasPending = existing.Status == (short)OrderStatus.Pending;
                var isNowPaid = status == (short)OrderStatus.Paid;
                
                // Chỉ cập nhật customer spent/rank nếu:
                // 1. Đơn hàng chuyển từ Pending → Paid
                // 2. Có customerId
                // 3. Payment method KHÔNG phải Cash (vì Cash đã cập nhật rồi khi tạo đơn)
                if (wasPending && isNowPaid && existing.CustomerId.HasValue && existing.TotalPrice.HasValue)
                {
                    // Kiểm tra payment method để tránh cập nhật 2 lần
                    var isCashPayment = existing.PaymentMethod == PaymentMethodEnum.Cash.ToString();
                    if (!isCashPayment)
                    {
                        Console.WriteLine($"Updating customer {existing.CustomerId.Value} spent with order total: {existing.TotalPrice.Value} (Non-Cash payment)");
                        await _customerService.UpdateCustomerSpentAndRankAsync(existing.CustomerId.Value, existing.TotalPrice.Value);
                    }
                    else
                    {
                        Console.WriteLine($"Skipping customer spent update for Cash payment order (already updated during order creation)");
                    }
                }

                existing.Status = status;
                var affected = await _orderRepo.UpdateAsync(existing);
                if (affected > 0)
                {
                    var response = _mapper.Map<OrderResponse>(existing);
                    return new ApiResponse<OrderResponse>
                    {
                        Success = true,
                        Message = "Order status updated successfully",
                        Data = response
                    };
                }
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = "Update status failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(long id)
        {
            try
            {
                var existing = await _orderRepo.GetByIdAsync(id);
                if (existing == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Order not found",
                        Data = false
                    };
                }
                var affected = await _orderRepo.RemoveAsync(existing);
                return new ApiResponse<bool>
                {
                    Success = affected,
                    Message = affected ? "Deleted successfully" : "Delete failed",
                    Data = affected
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = false
                };
            }
        }

        public async Task<PagedResponse<OrderResponse>> GetFilteredOrdersAsync(OrderGetRequest Filter, int page, int pageSize)
        {
            var filter = _mapper.Map<Order>(Filter);
            var query = _orderRepo.GetFiltered(filter);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResponse<OrderResponse>
            {
                Items = _mapper.Map<IEnumerable<OrderResponse>>(items),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<OrderResponse>> UpdateAsync(long id, OrderRequest request)
        {
            try
            {
                var existing = await _orderRepo.GetByIdAsync(id);
                if (existing == null)
                {
                    return new ApiResponse<OrderResponse>
                    {
                        Success = false,
                        Message = "Order not found",
                        Data = null
                    };
                }
                // Map các trường cơ bản
                _mapper.Map(request, existing);

                // Nếu status=Paid hoặc paymentMethod=Cash, thực hiện finalize tương tự Create
                var isCashUpdate = request.PaymentMethod == PaymentMethodEnum.Cash;
                var isPaidUpdate = request.Status == (short)OrderStatus.Paid;
                var shouldFinalizeUpdate = isCashUpdate || isPaidUpdate;

                var createdOrderDetails = new List<OrderDetailResponse>();
                decimal totalProductPrice = 0m;
                decimal grossRevenueTotal = 0m;
                var discountAmount = 0m;
                var voucherAmount = 0m;
                var rankBenefitAmount = 0m;
                var discountNotes = new List<string>();

                if (shouldFinalizeUpdate && request.OrderDetails != null && request.OrderDetails.Any())
                {
                    // Validate tồn kho trước
                    var stockValidationResults = new List<(Product Product, int QuantityToDeduct)>();
                    foreach (var od in request.OrderDetails)
                    {
                        if (od.ProductId.HasValue)
                        {
                            var product = await _productRepo.GetByIdAsync(od.ProductId.Value);
                            if (product != null && product.Quantity.HasValue)
                            {
                                int quantityToDeduct = od.Quantity ?? 0;
                                if (od.ProductUnitId.HasValue && od.ProductUnitId.Value > 0)
                                {
                                    var productUnit = await _productUnitRepo.GetByIdAsync(od.ProductUnitId.Value);
                                    if (productUnit?.ConversionFactor > 0)
                                    {
                                        quantityToDeduct = (int)(quantityToDeduct * productUnit.ConversionFactor.Value);
                                    }
                                }
                                if (product.Quantity.Value < quantityToDeduct)
                                {
                                    return new ApiResponse<OrderResponse>
                                    {
                                        Success = false,
                                        Message = $"Insufficient stock for product '{product.ProductName}'. Required={quantityToDeduct}, Available={product.Quantity.Value}",
                                        Data = null
                                    };
                                }
                                stockValidationResults.Add((product, quantityToDeduct));
                            }
                            else
                            {
                                return new ApiResponse<OrderResponse>
                                {
                                    Success = false,
                                    Message = $"Product with ID {od.ProductId.Value} not found or has no quantity",
                                    Data = null
                                };
                            }
                        }
                    }

                    // Tạo order detail và inventory transaction
                    foreach (var od in request.OrderDetails)
                    {
                        var odReq = new OrderDetailRequest
                        {
                            Quantity = od.Quantity,
                            ProductUnitId = od.ProductUnitId,
                            ProductId = od.ProductId
                        };
                        var odResult = await _orderDetailService.CreateAsync(odReq, existing.OrderId);
                        if (!odResult.Success || odResult.Data == null)
                        {
                            return new ApiResponse<OrderResponse>
                            {
                                Success = false,
                                Message = "Failed to create order detail",
                                Data = null
                            };
                        }
                        createdOrderDetails.Add(odResult.Data);
                        await CreateInventoryTransactionForOrderDetail(odReq, existing);
                    }

                    // Gross revenue và promotion price
                    foreach (var d in createdOrderDetails)
                    {
                        if (d.ProductId > 0 && d.Quantity > 0)
                        {
                            decimal originalUnitPrice = 0m;
                            if (d.ProductUnitId > 0)
                            {
                                var puOriginal = await _productUnitRepo.GetByIdAsync(d.ProductUnitId);
                                if (puOriginal?.Price != null) originalUnitPrice = puOriginal.Price.Value;
                            }
                            if (originalUnitPrice == 0m)
                            {
                                var p = await _productRepo.GetByIdAsync(d.ProductId);
                                if (p?.Price != null) originalUnitPrice = p.Price.Value;
                            }
                            if (originalUnitPrice > 0m) grossRevenueTotal += originalUnitPrice * d.Quantity;

                            if (d.BasePrice > 0)
                            {
                                var originalUnit = d.BasePrice / d.Quantity;
                                long? unitIdForPromo = null;
                                if (d.ProductUnitId > 0)
                                {
                                    var pu = await _productUnitRepo.GetByIdAsync(d.ProductUnitId);
                                    unitIdForPromo = pu?.UnitId;
                                }
                                var promoPrice = await CalculatePromotionUnitPriceAsync(d.ProductId, existing.ShopId, unitIdForPromo, originalUnit);
                                decimal newBase = d.BasePrice;
                                if (promoPrice.HasValue && promoPrice.Value < originalUnit)
                                {
                                    newBase = promoPrice.Value * d.Quantity;
                                    await _orderDetailService.UpdateBasePriceAsync(d.OrderDetailId, newBase);
                                }
                                d.BasePrice = newBase;
                                totalProductPrice += newBase;
                            }
                        }
                    }

                    existing.TotalPrice = totalProductPrice;
                    existing.GrossRevenue = grossRevenueTotal;

                    // Discount %
                    var normalizedDiscount = request.Discount ?? 0m;
                    if (normalizedDiscount < 0m) normalizedDiscount = 0m;
                    if (normalizedDiscount > 100m) normalizedDiscount = 100m;
                    existing.Discount = normalizedDiscount;
                    if (existing.Discount.HasValue && existing.Discount.Value > 0)
                    {
                        discountAmount = totalProductPrice * (existing.Discount.Value / 100m);
                        discountNotes.Add($"Giảm {existing.Discount.Value:F2}% từ chiết khấu: {discountAmount:N0} đ");
                    }

                    // Voucher
                    if (request.VoucherId.HasValue && request.VoucherId.Value > 0)
                    {
                        var voucher = await _voucherRepo.GetByIdAsync(request.VoucherId.Value);
                        if (voucher == null)
                        {
                            return new ApiResponse<OrderResponse> { Success = false, Message = "Voucher not found", Data = null };
                        }
                        if (voucher.Expired.HasValue && voucher.Expired.Value < DateTime.UtcNow)
                        {
                            return new ApiResponse<OrderResponse> { Success = false, Message = "Voucher expired", Data = null };
                        }
                        if (voucher.ShopId.HasValue && voucher.ShopId.Value != request.ShopId)
                        {
                            return new ApiResponse<OrderResponse> { Success = false, Message = "Voucher is not valid for this shop", Data = null };
                        }
                        if (voucher.Type == 1 && voucher.Value.HasValue)
                        {
                            voucherAmount = voucher.Value.Value;
                            discountNotes.Add($"Voucher {voucher.Code}: Giảm tổng đơn {voucherAmount:N0} đ");
                        }
                        else if (voucher.Type == 2 && voucher.Value.HasValue)
                        {
                            var rate = voucher.Value.Value;
                            if (rate < 0m) rate = 0m; if (rate > 100m) rate = 100m;
                            voucherAmount = totalProductPrice * (rate / 100m);
                            discountNotes.Add($"Voucher {voucher.Code}: Giảm {rate:F2}% = {voucherAmount:N0} đ");
                        }
                    }

                    // Rank benefit
                    if (request.CustomerId.HasValue)
                    {
                        var customer = await _customerRepo.GetByIdAsync(request.CustomerId.Value);
                        if (customer?.Rank?.Benefit.HasValue == true)
                        {
                            var rankBenefitRate = (decimal)customer.Rank.Benefit.Value;
                            rankBenefitAmount = totalProductPrice * rankBenefitRate;
                            discountNotes.Add($"Chiết khấu khách hàng {customer.Rank.RankName}: Giảm {rankBenefitRate * 100:F2}% = {rankBenefitAmount:N0} đ");
                        }
                    }

                    existing.TotalDiscount = discountAmount + voucherAmount + rankBenefitAmount;
                    existing.FinalPrice = existing.TotalPrice - existing.TotalDiscount;
                    if (existing.FinalPrice < 0) existing.FinalPrice = 0;
                    if (discountNotes.Any()) existing.Note = string.Join("; ", discountNotes);

                    // Phân bổ discount toàn cục vào từng OrderDetail và tính profit
                    if ((existing.TotalDiscount ?? 0m) > 0 && totalProductPrice > 0 && createdOrderDetails.Any())
                    {
                        var discountRatio = totalProductPrice > 0 ? ((existing.TotalDiscount ?? 0m) / totalProductPrice) : 0m;
                        foreach (var orderDetail in createdOrderDetails)
                        {
                            if (orderDetail.BasePrice > 0)
                            {
                                var itemDiscount = orderDetail.BasePrice * discountRatio;
                                var finalPrice = orderDetail.BasePrice - itemDiscount;

                                // Tính profit = finalPrice - (cost theo đơn vị) * quantity
                                var product = await _productRepo.GetByIdAsync(orderDetail.ProductId);
                                var cost = product?.Cost ?? 0m;
                                var quantity = orderDetail.Quantity;
                                decimal conversionFactor = 1m;
                                if (orderDetail.ProductUnitId > 0)
                                {
                                    var puForCost = await _productUnitRepo.GetByIdAsync(orderDetail.ProductUnitId);
                                    if (puForCost?.ConversionFactor != null && puForCost.ConversionFactor.Value > 0)
                                        conversionFactor = puForCost.ConversionFactor.Value;
                                }
                                var profit = finalPrice - (cost * conversionFactor * quantity);

                                // Cập nhật DB và đồng bộ response
                                await _orderDetailService.UpdateOrderDetailPricingAsync(orderDetail.OrderDetailId, itemDiscount, finalPrice, profit);
                                orderDetail.DiscountAmount = itemDiscount;
                                orderDetail.FinalPrice = finalPrice;
                                orderDetail.Profit = profit;

                                // Cập nhật InventoryTransaction profit
                                await UpdateInventoryTransactionProfit(orderDetail.OrderDetailId, profit);
                            }
                        }
                    }

                    // Trừ tồn kho
                    foreach (var (product, qty) in stockValidationResults)
                    {
                        product.Quantity = Math.Max(0, (product.Quantity.Value - qty));
                        var threshold = product.IsLow ?? 0;
                        var currentQty = product.Quantity ?? 0;
                        if (currentQty > threshold && (product.IsLowStockNotified ?? false))
                        {
                            product.IsLowStockNotified = false;
                        }
                        await _productRepo.UpdateAsync(product);
                    }
                }

                var affected = await _orderRepo.UpdateAsync(existing);
                if (affected > 0)
                {
                    // Cập nhật spent/email khi finalize trong update
                    if (shouldFinalizeUpdate && existing.CustomerId.HasValue && existing.FinalPrice > 0)
                    {
                        await _customerService.UpdateCustomerSpentAndRankAsync(existing.CustomerId.Value, existing.FinalPrice ?? 0);
                        if ((request.IsSendInvoice ?? false))
                        {
                            await SendOrderConfirmationEmailAsync(existing, createdOrderDetails);
                        }
                    }

                    var response = _mapper.Map<OrderResponse>(existing);
                    response.OrderDetails = createdOrderDetails.Any() ? createdOrderDetails : response.OrderDetails;
                    return new ApiResponse<OrderResponse>
                    {
                        Success = true,
                        Message = "Update successfully",
                        Data = response
                    };
                }
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = "Update failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<OrderResponse>> CancelOrderAsync(long id, string reason = "Đơn hàng hết hạn thanh toán")
        {
            try
            {
                // Load Order với OrderDetails để có thể hoàn lại tồn kho
                var existing = await _orderRepo.GetFiltered(new Order { OrderId = id })
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.ProductUnit)
                    .FirstOrDefaultAsync();
                    
                if (existing == null)
                {
                    return new ApiResponse<OrderResponse>
                    {
                        Success = false,
                        Message = "Order not found",
                        Data = null
                    };
                }

                // Kiểm tra xem đơn hàng có thể hủy không (chỉ hủy được khi status = 0 - chờ thanh toán)
                if (existing.Status != 0)
                {
                    return new ApiResponse<OrderResponse>
                    {
                        Success = false,
                        Message = $"Cannot cancel order with status {existing.Status}. Only pending orders can be cancelled.",
                        Data = null
                    };
                }

                // Xóa các InventoryTransaction đã tạo khi tạo đơn hàng
                await DeleteInventoryTransactionsForOrder(existing.OrderId);
                
                // Hoàn lại số lượng sản phẩm đã trừ tồn kho
                if (existing.OrderDetails != null && existing.OrderDetails.Any())
                {
                    foreach (var orderDetail in existing.OrderDetails)
                    {
                        if (orderDetail.ProductId > 0)
                        {
                            var product = await _productRepo.GetByIdAsync(orderDetail.ProductId);
                            if (product != null && product.Quantity.HasValue)
                            {
                                // Tính số lượng cần hoàn lại (có tính conversion factor)
                                int quantityToRestore = orderDetail.Quantity;
                                
                                if (orderDetail.ProductUnitId > 0)
                                {
                                    var productUnit = await _productUnitRepo.GetByIdAsync(orderDetail.ProductUnitId);
                                    if (productUnit != null && productUnit.ConversionFactor.HasValue && productUnit.ConversionFactor.Value > 0)
                                    {
                                        quantityToRestore = (int)(quantityToRestore * productUnit.ConversionFactor.Value);
                                    }
                                }
                                
                                // Cộng lại số lượng vào tồn kho
                                product.Quantity = product.Quantity.Value + quantityToRestore;
                                await _productRepo.UpdateAsync(product);
                                
                                Console.WriteLine($"Hoàn lại {quantityToRestore} {product.ProductName} vào tồn kho. Tồn kho hiện tại: {product.Quantity.Value}");
                            }
                        }
                    }
                }

                // Cập nhật status thành 2 (Đã hủy) và note với lý do hủy
                existing.Status = 2;
                if (!string.IsNullOrEmpty(reason))
                {
                    existing.Note = string.IsNullOrEmpty(existing.Note) ? reason : $"{existing.Note} | {reason}";
                }

                var affected = await _orderRepo.UpdateAsync(existing);
                if (affected > 0)
                {
                    var response = _mapper.Map<OrderResponse>(existing);
                    return new ApiResponse<OrderResponse>
                    {
                        Success = true,
                        Message = "Order cancelled successfully",
                        Data = response
                    };
                }
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = "Cancel order failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }

        private async Task CreateInventoryTransactionForOrderDetail(OrderDetailRequest orderDetailRequest, Order order)
        {
            try
            {
                // Chỉ tạo InventoryTransaction nếu có ProductUnitId
                if (orderDetailRequest.ProductUnitId.HasValue && orderDetailRequest.ProductUnitId.Value > 0)
                {
                    // Lấy thông tin ProductUnit để tính conversion factor
                    var productUnit = await _productUnitRepo.GetByIdAsync(orderDetailRequest.ProductUnitId.Value);
                    if (productUnit != null && productUnit.ProductId.HasValue)
                    {
                        // Tính số lượng sản phẩm cần trừ (quantity * conversion factor)
                        int quantityToDeduct = orderDetailRequest.Quantity ?? 0;
                        if (productUnit.ConversionFactor.HasValue && productUnit.ConversionFactor.Value > 0)
                        {
                            quantityToDeduct = (int)(quantityToDeduct * productUnit.ConversionFactor.Value);
                        }

                        // Tạo InventoryTransaction với type = 1 (Sale)
                        var inventoryTransactionRequest = new InventoryTransactionRequest
                        {
                            Type = (int)InventoryTransactionType.Sale, // Sale
                            ProductId = productUnit.ProductId.Value,
                            OrderId = order.OrderId,
                            UnitId = productUnit.UnitId ?? 0,
                            Quantity = quantityToDeduct,
                            Price =  0,
                            ShopId = order.ShopId ?? 0
                        };

                        await _inventoryTransactionService.CreateAsync(inventoryTransactionRequest);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không throw để không ảnh hưởng đến việc tạo Order
                // Trong thực tế, bạn có thể sử dụng ILogger để log lỗi
                Console.WriteLine($"Error creating inventory transaction: {ex.Message}");
            }
        }

        private async Task DeleteInventoryTransactionsForOrder(long orderId)
        {
            try
            {
                // Sử dụng context trực tiếp để tìm và xóa InventoryTransaction
                var inventoryTransactions = await _context.InventoryTransactions
                    .Where(it => it.OrderId == orderId)
                    .ToListAsync();
                
                if (inventoryTransactions.Any())
                {
                    _context.InventoryTransactions.RemoveRange(inventoryTransactions);
                    await _context.SaveChangesAsync();
                    
                    Console.WriteLine($"Đã xóa {inventoryTransactions.Count} InventoryTransaction của đơn hàng #{orderId}");
                }
                else
                {
                    Console.WriteLine($"Không tìm thấy InventoryTransaction nào cho đơn hàng #{orderId}");
                }
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không throw để không ảnh hưởng đến việc hủy Order
                Console.WriteLine($"Error deleting inventory transactions for order #{orderId}: {ex.Message}");
            }
        }

        private async Task SendOrderConfirmationEmailAsync(Order order, List<OrderDetailResponse> orderDetails)
        {
            try
            {
                // Lấy thông tin khách hàng
                var customer = await _customerRepo.GetByIdAsync(order.CustomerId.Value);
                if (customer == null || string.IsNullOrEmpty(customer.Email))
                {
                    Console.WriteLine($"Không thể gửi email cho Order #{order.OrderId}: Khách hàng không có email");
                    return;
                }

                // Lấy thông tin shop
                var shop = await _shopRepo.GetByIdAsync(order.ShopId.Value);
                if (shop == null)
                {
                    Console.WriteLine($"Không thể gửi email cho Order #{order.OrderId}: Không tìm thấy thông tin shop");
                    return;
                }

                // Tạo HTML table cho chi tiết sản phẩm
                var orderDetailsHtml = await GenerateOrderDetailsHtmlTableAsync(orderDetails);

                // Gửi email xác nhận đơn hàng
                var emailSent = await _emailService.SendOrderConfirmationEmailAsync(
                    customer.Email,
                    customer.FullName,
                    order.OrderId,
                    shop.ShopName,
                    orderDetailsHtml,
                    order.TotalPrice ?? 0,
                    order.TotalDiscount,
                    order.FinalPrice ?? 0,
                    order.Datetime ?? DateTime.UtcNow,
                    order.Note
                );
                
                if (emailSent)
                {
                    Console.WriteLine($"✅ Đã gửi email xác nhận đơn hàng #{order.OrderId} đến {customer.Email}");
                }
                else
                {
                    Console.WriteLine($"❌ Không thể gửi email xác nhận đơn hàng #{order.OrderId} đến {customer.Email}");
                }
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không throw để không ảnh hưởng đến việc tạo Order
                Console.WriteLine($"Error sending order confirmation email for Order #{order.OrderId}: {ex.Message}");
            }
        }

        private async Task<string> GenerateOrderDetailsHtmlTableAsync(List<OrderDetailResponse> orderDetails)
        {
            var content = new StringBuilder();
            
            content.AppendLine($"<table style='width:100%;border-collapse:collapse;border:1px solid #e2e8f0;'>");
            content.AppendLine($"<thead>");
            content.AppendLine($"<tr style='background:#f8fafc;'>");
            content.AppendLine($"<th style='padding:12px;text-align:left;border:1px solid #e2e8f0;font-size:14px;color:#334155;'>Sản phẩm</th>");
            content.AppendLine($"<th style='padding:12px;text-align:center;border:1px solid #e2e8f0;font-size:14px;color:#334155;'>Số lượng</th>");
            content.AppendLine($"<th style='padding:12px;text-align:right;border:1px solid #e2e8f0;font-size:14px;color:#334155;'>Đơn giá</th>");
            content.AppendLine($"<th style='padding:12px;text-align:right;border:1px solid #e2e8f0;font-size:14px;color:#334155;'>Thành tiền</th>");
            content.AppendLine($"</tr>");
            content.AppendLine($"</thead>");
            content.AppendLine($"<tbody>");

            foreach (var detail in orderDetails)
            {
                // Lấy thông tin sản phẩm
                var product = await _productRepo.GetByIdAsync(detail.ProductId);
                var productName = product?.ProductName ?? $"Sản phẩm ID: {detail.ProductId}";
                
                content.AppendLine($"<tr>");
                content.AppendLine($"<td style='padding:12px;border:1px solid #e2e8f0;font-size:14px;color:#0f172a;'>{productName}</td>");
                content.AppendLine($"<td style='padding:12px;text-align:center;border:1px solid #e2e8f0;font-size:14px;color:#0f172a;'>{detail.Quantity}</td>");
                content.AppendLine($"<td style='padding:12px;text-align:right;border:1px solid #e2e8f0;font-size:14px;color:#0f172a;'>{((detail.FinalPrice / detail.Quantity)):N0} đ</td>");
                content.AppendLine($"<td style='padding:12px;text-align:right;border:1px solid #e2e8f0;font-size:14px;color:#0f172a;font-weight:600;'>{detail.FinalPrice:N0} đ</td>");
                content.AppendLine($"</tr>");
            }

            content.AppendLine($"</tbody>");
            content.AppendLine($"</table>");

            return content.ToString();
        }

        private async Task UpdateInventoryTransactionProfit(long orderDetailId, decimal profit)
        {
            try
            {
                // Lấy OrderDetail để tìm OrderId
                var orderDetail = await _orderDetailRepo.GetByIdAsync(orderDetailId);
                if (orderDetail?.OrderId == null) return;
                
                // Tìm InventoryTransaction tương ứng với OrderId
                var inventoryTransaction = await _context.InventoryTransactions
                    .FirstOrDefaultAsync(it => it.OrderId == orderDetail.OrderId);
                
                if (inventoryTransaction != null)
                {
                    inventoryTransaction.Price = profit; // Sử dụng Price field để lưu profit
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Updated InventoryTransaction {inventoryTransaction.InventoryTransactionId} Price (profit): {profit:N0}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating inventory transaction price (profit): {ex.Message}");
            }
        }
    }
}
