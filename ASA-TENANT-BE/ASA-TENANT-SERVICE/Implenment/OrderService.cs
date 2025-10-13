using ASA_TENANT_REPO.Models;
using ASA_TENANT_REPO.Repository;
using ASA_TENANT_REPO.Models;
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
        public OrderService(OrderRepo orderRepo, IOrderDetailService orderDetailService, IInventoryTransactionService inventoryTransactionService, ProductUnitRepo productUnitRepo, IMapper mapper, VoucherRepo voucherRepo, ProductRepo productRepo, INotificationService notificationService, IFcmService fcmService, IRealtimeNotifier realtimeNotifier, UserRepo userRepo, UnitRepo unitRepo, ICustomerService customerService, CustomerRepo customerRepo)
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
                
                // Set status theo phương thức thanh toán
                // Status: 0 = Chờ thanh toán, 1 = Đã thanh toán, 2 = Đã hủy
                var isCash = request.PaymentMethod == PaymentMethodEnum.Cash;
                entity.Status = (short)(isCash ? OrderStatus.Paid : OrderStatus.Pending);
                
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
                // Nếu có OrderDetails, bỏ qua TotalPrice từ request vì sẽ tính tự động
                if (request.OrderDetails != null && request.OrderDetails.Any())
                {
                    entity.TotalPrice = 0; // Sẽ được cập nhật sau khi tạo OrderDetails
                }
                
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

                // Tính tổng discount trước khi tạo OrderDetails
                var discountRate = request.Discount ?? 0m;
                var rankBenefitRate = 0m;
                var rankBenefitNote = "";
                
                // Lấy benefit từ customer rank nếu có customerId
                if (request.CustomerId.HasValue)
                {
                    var customer = await _customerRepo.GetByIdAsync(request.CustomerId.Value);
                    
                    if (customer?.Rank?.Benefit.HasValue == true)
                    {
                        rankBenefitRate = (decimal)customer.Rank.Benefit.Value * 100; // Convert từ 0.02 thành 2%
                        rankBenefitNote = $"Giảm {rankBenefitRate:F2}% cho khách hàng rank {customer.Rank.RankName}";
                        
                        Console.WriteLine($"Customer {customer.CustomerId} rank {customer.Rank.RankName} có benefit {rankBenefitRate:F2}%");
                    }
                }
                
                // Cộng discount từ request và benefit từ rank
                var totalDiscountRate = discountRate + rankBenefitRate;
                if (totalDiscountRate < 0m) totalDiscountRate = 0m;
                if (totalDiscountRate > 100m) totalDiscountRate = 100m;
                
                // Lưu tổng discount vào entity
                entity.Discount = totalDiscountRate;

                // Tạo OrderDetails nếu có và tính tổng TotalPrice
                var createdOrderDetails = new List<OrderDetailResponse>();
                decimal totalOrderPrice = 0;
                
                if (request.OrderDetails != null && request.OrderDetails.Any())
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
                            // Áp dụng discount cho từng OrderDetail
                            if (orderDetailResult.Data.TotalPrice.HasValue && totalDiscountRate > 0m)
                            {
                                var originalPrice = orderDetailResult.Data.TotalPrice.Value;
                                var discountedPrice = originalPrice * (1m - totalDiscountRate / 100m);
                                
                                // Cập nhật TotalPrice của OrderDetail với giá đã giảm
                                orderDetailResult.Data.TotalPrice = discountedPrice;
                                
                                // Cập nhật OrderDetail trong database với giá đã giảm
                                await _orderDetailService.UpdateTotalPriceAsync(orderDetailResult.Data.OrderDetailId, discountedPrice);
                                
                                Console.WriteLine($"OrderDetail {orderDetailResult.Data.OrderDetailId}: Giá gốc {originalPrice:C0} → Giá sau giảm {discountedPrice:C0} (giảm {totalDiscountRate:F2}%)");
                            }
                            
                            createdOrderDetails.Add(orderDetailResult.Data);
                            // Cộng dồn TotalPrice của OrderDetail vào tổng Order (đã giảm giá)
                            if (orderDetailResult.Data.TotalPrice.HasValue)
                            {
                                totalOrderPrice += orderDetailResult.Data.TotalPrice.Value;
                            }

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
                    
                    // BƯỚC 3: TRỪ TỒN KHO SAU KHI TẤT CẢ ĐÃ THÀNH CÔNG
                    foreach (var (product, quantityToDeduct) in stockValidationResults)
                    {
                        product.Quantity = Math.Max(0, (product.Quantity.Value - quantityToDeduct));
                        await _productRepo.UpdateAsync(product);

                        // Low stock check and notify
                        var threshold = product.IsLow ?? 0;
                        var currentQty = product.Quantity ?? 0;
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

                // Cập nhật TotalPrice của Order nếu có OrderDetails
                if (createdOrderDetails.Any())
                {
                    entity.TotalPrice = totalOrderPrice;
                }

                // Thêm thông tin giảm giá vào note nếu có rank benefit
                if (!string.IsNullOrEmpty(rankBenefitNote))
                {
                    if (string.IsNullOrEmpty(entity.Note) || entity.Note == "null")
                    {
                        entity.Note = rankBenefitNote;
                    }
                    else
                    {
                        entity.Note = $"{entity.Note} | {rankBenefitNote}";
                    }
                }

                // Áp dụng voucher nếu có (kiểm tra hạn và đúng Shop)
                if (request.VoucherId.HasValue)
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

                    if (voucher.Value.HasValue)
                    {
                        var currentTotal = entity.TotalPrice ?? 0m;
                        decimal finalTotal = currentTotal;
                        if (voucher.Type == 1)
                        {
                            // Trừ số tiền cố định
                            finalTotal = currentTotal - voucher.Value.Value;
                        }
                        else if (voucher.Type == 2)
                        {
                            // Trừ theo phần trăm (0..100%)
                            var rate = voucher.Value.Value;
                            if (rate < 0m) rate = 0m;
                            if (rate > 100m) rate = 100m;
                            finalTotal = currentTotal * (1m - rate / 100m);
                        }
                        if (finalTotal < 0m) finalTotal = 0m;
                        entity.TotalPrice = finalTotal;
                    }
                }

                // Lưu cập nhật TotalPrice cuối cùng
                await _orderRepo.UpdateAsync(entity);

                // Nếu payment method là Cash và có customerId, cập nhật spent và rank của customer
                if (isCash && entity.CustomerId.HasValue && entity.TotalPrice.HasValue)
                {
                    await _customerService.UpdateCustomerSpentAndRankAsync(entity.CustomerId.Value, entity.TotalPrice.Value);
                }

                var response = _mapper.Map<OrderResponse>(entity);
                response.OrderDetails = createdOrderDetails;
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
                
                if (wasPending && isNowPaid && existing.CustomerId.HasValue && existing.TotalPrice.HasValue)
                {
                    // Cập nhật spent và rank của customer
                    await _customerService.UpdateCustomerSpentAndRankAsync(existing.CustomerId.Value, existing.TotalPrice.Value);
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
                _mapper.Map(request, existing);
                var affected = await _orderRepo.UpdateAsync(existing);
                if (affected > 0)
                {
                    var response = _mapper.Map<OrderResponse>(existing);
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
                            Type = 1, // Sale
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
    }
}
