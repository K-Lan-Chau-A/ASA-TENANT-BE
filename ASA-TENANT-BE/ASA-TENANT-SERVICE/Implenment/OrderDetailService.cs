using ASA_TENANT_REPO.Models;
using ASA_TENANT_REPO.Repository;
using ASA_TENANT_SERVICE.DTOs.Common;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Interface;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Implenment
{
    public class OrderDetailService : IOrderDetailService
    {
        private readonly OrderDetailRepo _orderDetailRepo;
        private readonly ProductUnitRepo _productUnitRepo;
        private readonly ProductRepo _productRepo;
        private readonly IMapper _mapper;
        public OrderDetailService(OrderDetailRepo orderDetailRepo, ProductUnitRepo productUnitRepo, ProductRepo productRepo, IMapper mapper)
        {
            _orderDetailRepo = orderDetailRepo;
            _productUnitRepo = productUnitRepo;
            _productRepo = productRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse<OrderDetailResponse>> CreateAsync(OrderDetailRequest request)
        {
            try
            {
                var entity = _mapper.Map<OrderDetail>(request);
                decimal unitPrice = 0;
                if (request.ProductUnitId.HasValue && request.ProductUnitId.Value > 0)
                {
                    var productUnit = await _productUnitRepo.GetByIdAsync(request.ProductUnitId);
                    if (productUnit != null && productUnit.Price.HasValue)
                    {
                        unitPrice = productUnit.Price.Value;
                    }
                }
                if (unitPrice == 0 && request.ProductId.HasValue && request.ProductId.Value > 0)
                {
                    var product = await _productRepo.GetByIdAsync(request.ProductId);
                    if (product != null && product.Price.HasValue)
                    {
                        unitPrice = product.Price.Value;
                    }
                }
                if (request.Quantity.HasValue && unitPrice > 0)
                {
                    entity.BasePrice = unitPrice * request.Quantity.Value;
                    entity.DiscountAmount = 0;
                    entity.FinalPrice = entity.BasePrice;
                    entity.Profit = 0;
                }
                var affected = await _orderDetailRepo.CreateAsync(entity);
                if (affected > 0)
                {
                    var response = _mapper.Map<OrderDetailResponse>(entity);
                    return new ApiResponse<OrderDetailResponse>
                    {
                        Success = true,
                        Message = "Create successfully",
                        Data = response
                    };
                }
                return new ApiResponse<OrderDetailResponse>
                {
                    Success = false,
                    Message = "Create failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<OrderDetailResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<OrderDetailResponse>> CreateAsync(OrderDetailRequest request, long orderId)
        {
            try
            {
                var entity = _mapper.Map<OrderDetail>(request);
                entity.OrderId = orderId; // Set OrderId
                
                decimal unitPrice = 0;
                if (request.ProductUnitId.HasValue && request.ProductUnitId.Value > 0)
                {
                    var productUnit = await _productUnitRepo.GetByIdAsync(request.ProductUnitId);
                    if (productUnit != null && productUnit.Price.HasValue)
                    {
                        unitPrice = productUnit.Price.Value;
                    }
                }
                if (unitPrice == 0 && request.ProductId.HasValue && request.ProductId.Value > 0)
                {
                    var product = await _productRepo.GetByIdAsync(request.ProductId);
                    if (product != null && product.Price.HasValue)
                    {
                        unitPrice = product.Price.Value;
                    }
                }
                if (request.Quantity.HasValue && unitPrice > 0)
                {
                    entity.BasePrice = unitPrice * request.Quantity.Value;
                    entity.DiscountAmount = 0;
                    entity.FinalPrice = entity.BasePrice;
                    entity.Profit = 0;
                }
                var affected = await _orderDetailRepo.CreateAsync(entity);
                if (affected > 0)
                {
                    var response = _mapper.Map<OrderDetailResponse>(entity);
                    return new ApiResponse<OrderDetailResponse>
                    {
                        Success = true,
                        Message = "Create successfully",
                        Data = response
                    };
                }
                return new ApiResponse<OrderDetailResponse>
                {
                    Success = false,
                    Message = "Create failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<OrderDetailResponse>
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
                var existing = await _orderDetailRepo.GetByIdAsync(id);
                if (existing == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "OrderDetail not found",
                        Data = false
                    };
                }
                var affected = await _orderDetailRepo.RemoveAsync(existing);
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

        public async Task<PagedResponse<OrderDetailResponse>> GetFilteredOrderDetailsAsync(OrderDetailGetRequest Filter, int page, int pageSize)
        {
            var filter = _mapper.Map<OrderDetail>(Filter);
            var query = _orderDetailRepo.GetFiltered(filter);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResponse<OrderDetailResponse>
            {
                Items = _mapper.Map<IEnumerable<OrderDetailResponse>>(items),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<OrderDetailResponse>> UpdateAsync(long id, OrderDetailRequest request)
        {
            try
            {
                var existing = await _orderDetailRepo.GetByIdAsync(id);
                if (existing == null)
                {
                    return new ApiResponse<OrderDetailResponse>
                    {
                        Success = false,
                        Message = "OrderDetail not found",
                        Data = null
                    };
                }
                _mapper.Map(request, existing);
                var affected = await _orderDetailRepo.UpdateAsync(existing);
                if (affected > 0)
                {
                    var response = _mapper.Map<OrderDetailResponse>(existing);
                    return new ApiResponse<OrderDetailResponse>
                    {
                        Success = true,
                        Message = "Update successfully",
                        Data = response
                    };
                }
                return new ApiResponse<OrderDetailResponse>
                {
                    Success = false,
                    Message = "Update failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<OrderDetailResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<bool>> UpdateFinalPriceAsync(long orderDetailId, decimal newFinalPrice)
        {
            try
            {
                var existing = await _orderDetailRepo.GetByIdAsync(orderDetailId);
                if (existing == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "OrderDetail not found",
                        Data = false
                    };
                }

                existing.FinalPrice = newFinalPrice;
                var affected = await _orderDetailRepo.UpdateAsync(existing);
                
                return new ApiResponse<bool>
                {
                    Success = affected > 0,
                    Message = affected > 0 ? "OrderDetail total price updated successfully" : "Update failed",
                    Data = affected > 0
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

        public async Task<ApiResponse<bool>> UpdateOrderDetailPricingAsync(long orderDetailId, decimal discountAmount, decimal finalPrice, decimal profit)
        {
            try
            {
                var existing = await _orderDetailRepo.GetByIdAsync(orderDetailId);
                if (existing == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "OrderDetail not found",
                        Data = false
                    };
                }

                existing.DiscountAmount = discountAmount;
                existing.FinalPrice = finalPrice;
                existing.Profit = profit;
                
                var affected = await _orderDetailRepo.UpdateAsync(existing);
                
                return new ApiResponse<bool>
                {
                    Success = affected > 0,
                    Message = affected > 0 ? "OrderDetail pricing updated successfully" : "Update failed",
                    Data = affected > 0
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
    }
}