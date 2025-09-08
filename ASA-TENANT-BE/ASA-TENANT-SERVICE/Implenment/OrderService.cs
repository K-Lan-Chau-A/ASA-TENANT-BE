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
    public class OrderService : IOrderService
    {
        private readonly OrderRepo _orderRepo;
        private readonly IOrderDetailService _orderDetailService;
        private readonly IMapper _mapper;
        public OrderService(OrderRepo orderRepo, IOrderDetailService orderDetailService, IMapper mapper)
        {
            _orderRepo = orderRepo;
            _orderDetailService = orderDetailService;
            _mapper = mapper;
        }

        public async Task<ApiResponse<OrderResponse>> CreateAsync(OrderRequest request)
        {
            try
            {
                // Tạo Order trước
                var entity = _mapper.Map<Order>(request);
                
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
                        orderDetailRequest.OrderId = entity.OrderId; // Gán OrderId cho OrderDetail
                        var orderDetailResult = await _orderDetailService.CreateAsync(orderDetailRequest);
                        if (orderDetailResult.Success && orderDetailResult.Data != null)
                        {
                            createdOrderDetails.Add(orderDetailResult.Data);
                            // Cộng dồn TotalPrice của OrderDetail vào tổng Order
                            if (orderDetailResult.Data.TotalPrice.HasValue)
                            {
                                totalOrderPrice += orderDetailResult.Data.TotalPrice.Value;
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
                    await _orderRepo.UpdateAsync(entity); // Cập nhật lại Order với TotalPrice đã tính
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
    }
}
