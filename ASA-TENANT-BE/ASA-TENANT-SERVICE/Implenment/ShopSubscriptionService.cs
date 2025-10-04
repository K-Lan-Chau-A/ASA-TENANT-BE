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
    public class ShopSubscriptionService : IShopSubscriptionService
    {
        private readonly ShopSubscriptionRepo _shopSubscriptionRepo;
        private readonly IMapper _mapper;
        public ShopSubscriptionService(ShopSubscriptionRepo shopSubscriptionRepo, IMapper mapper)
        {
            _shopSubscriptionRepo = shopSubscriptionRepo;
            _mapper = mapper;
        }

        public async Task<PagedResponse<ShopSubscriptionResponse>> GetFilteredAsync(ShopSubscriptionGetRequest filter, int page, int pageSize)
        {
            var entityFilter = _mapper.Map<ShopSubscription>(filter);
            var query = _shopSubscriptionRepo.GetFiltered(entityFilter);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResponse<ShopSubscriptionResponse>
            {
                Items = _mapper.Map<IEnumerable<ShopSubscriptionResponse>>(items),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<ShopSubscriptionResponse>> CreateAsync(ShopSubscriptionRequest request)
        {
            try
            {
                var entity = _mapper.Map<ShopSubscription>(request);
                var affected = await _shopSubscriptionRepo.CreateAsync(entity);
                if (affected > 0)
                {
                    var response = _mapper.Map<ShopSubscriptionResponse>(entity);
                    return new ApiResponse<ShopSubscriptionResponse>
                    {
                        Success = true,
                        Message = "Create successfully",
                        Data = response
                    };
                }
                return new ApiResponse<ShopSubscriptionResponse>
                {
                    Success = false,
                    Message = "Create failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ShopSubscriptionResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<ShopSubscriptionResponse>> UpdateAsync(long id, ShopSubscriptionRequest request)
        {
            try
            {
                var existing = await _shopSubscriptionRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<ShopSubscriptionResponse>
                    {
                        Success = false,
                        Message = "ShopSubscription not found",
                        Data = null
                    };

                _mapper.Map(request, existing);

                var affected = await _shopSubscriptionRepo.UpdateAsync(existing);
                if (affected > 0)
                {
                    var response = _mapper.Map<ShopSubscriptionResponse>(existing);
                    return new ApiResponse<ShopSubscriptionResponse>
                    {
                        Success = true,
                        Message = "Update successfully",
                        Data = response
                    };
                }
                return new ApiResponse<ShopSubscriptionResponse>
                {
                    Success = false,
                    Message = "Update failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ShopSubscriptionResponse>
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
                var existing = await _shopSubscriptionRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "ShopSubscription not found",
                        Data = false
                    };

                var affected = await _shopSubscriptionRepo.RemoveAsync(existing);
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
    }
}
