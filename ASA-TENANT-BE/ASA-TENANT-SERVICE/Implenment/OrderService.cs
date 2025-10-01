using ASA_TENANT_REPO.Models;
using ASA_TENANT_REPO.Repository;
using ASA_TENANT_SERVICE.DTOs.Common;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Interface;
using AutoMapper;
using ASA_TENANT_SERVICE.Enums;
using Microsoft.EntityFrameworkCore;
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
        public OrderService(OrderRepo orderRepo, IOrderDetailService orderDetailService, IInventoryTransactionService inventoryTransactionService, ProductUnitRepo productUnitRepo, IMapper mapper, VoucherRepo voucherRepo, ProductRepo productRepo)
        {
            _orderRepo = orderRepo;
            _orderDetailService = orderDetailService;
            _inventoryTransactionService = inventoryTransactionService;
            _productUnitRepo = productUnitRepo;
            _mapper = mapper;
            _voucherRepo = voucherRepo;
            _productRepo = productRepo;
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

                // Tạo OrderDetails nếu có và tính tổng TotalPrice
                var createdOrderDetails = new List<OrderDetailResponse>();
                decimal totalOrderPrice = 0;
                
                if (request.OrderDetails != null && request.OrderDetails.Any())
                {
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
                            // Cộng dồn TotalPrice của OrderDetail vào tổng Order
                            if (orderDetailResult.Data.TotalPrice.HasValue)
                            {
                                totalOrderPrice += orderDetailResult.Data.TotalPrice.Value;
                            }

                        // Tạo InventoryTransaction cho OrderDetail này
                        await CreateInventoryTransactionForOrderDetail(orderDetailWithOrderId, entity);

                        // Trước khi trừ tồn kho, kiểm tra số lượng đủ
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
                                // Nếu tồn kho không đủ, báo lỗi và dừng tạo order
                                if (product.Quantity.Value < quantityToDeduct)
                                {
                                    return new ApiResponse<OrderResponse>
                                    {
                                        Success = false,
                                        Message = $"Insufficient stock for product '{product.ProductName}'. Required={quantityToDeduct}, Available={product.Quantity.Value}",
                                        Data = null
                                    };
                                }
                                product.Quantity = Math.Max(0, (product.Quantity.Value - quantityToDeduct));
                                await _productRepo.UpdateAsync(product);
                            }
                        }
                        }
                        // Nếu tạo OrderDetail thất bại, có thể rollback Order hoặc chỉ log lỗi
                        // Ở đây tôi sẽ log lỗi nhưng vẫn trả về Order đã tạo
                        // Trong thực tế, bạn có thể sử dụng transaction để rollback
                    }
                }

                // Cập nhật TotalPrice của Order nếu có OrderDetails
                if (createdOrderDetails.Any())
                {
                    entity.TotalPrice = totalOrderPrice;
                }

                // Áp dụng discount (0..100%) nếu có
                var discountRate = request.Discount ?? 0m;
                if (discountRate < 0m) discountRate = 0m;
                if (discountRate > 100m) discountRate = 100m;
                if (entity.TotalPrice.HasValue)
                {
                    entity.TotalPrice = entity.TotalPrice.Value * (1m - discountRate / 100m);
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
