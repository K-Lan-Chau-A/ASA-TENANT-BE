using ASA_TENANT_REPO.DBContext;
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
    public class PromotionService : IPromotionService
    {
        private readonly PromotionRepo _promotionRepo;
        private readonly IMapper _mapper;
        public PromotionService(PromotionRepo promotionRepo, IMapper mapper)
        {
            _promotionRepo = promotionRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse<PromotionResponse>> CreateAsync(PromotionRequest request)
        {
            try
            {
                var entity = _mapper.Map<Promotion>(request);

                var affected = await _promotionRepo.CreateAsync(entity);

                if (affected > 0)
                {
                    var response = _mapper.Map<PromotionResponse>(entity);
                    return new ApiResponse<PromotionResponse>
                    {
                        Success = true,
                        Message = "Create successfully",
                        Data = response
                    };
                }

                return new ApiResponse<PromotionResponse>
                {
                    Success = false,
                    Message = "Create failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<PromotionResponse>
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
                var existing = await _promotionRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Promotion not found",
                        Data = false
                    };

                var affected = await _promotionRepo.RemoveAsync(existing);
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

        public async Task<PagedResponse<PromotionResponse>> GetFilteredPromotionsAsync(PromotionGetRequest Filter, int page, int pageSize)
        {
            var filter = _mapper.Map<Promotion>(Filter);
            var query = _promotionRepo.GetFiltered(filter);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResponse<PromotionResponse>
            {
                Items = _mapper.Map<IEnumerable<PromotionResponse>>(items),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<PromotionResponse>> UpdateAsync(long id, PromotionRequest request)
        {
            try
            {
                var existing = await _promotionRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<PromotionResponse>
                    {
                        Success = false,
                        Message = "Promotion not found",
                        Data = null
                    };

                _mapper.Map(request, existing);

                var affected = await _promotionRepo.UpdateAsync(existing);
                if (affected > 0)
                {
                    var response = _mapper.Map<PromotionResponse>(existing);
                    return new ApiResponse<PromotionResponse>
                    {
                        Success = true,
                        Message = "Update successfully",
                        Data = response
                    };
                }

                return new ApiResponse<PromotionResponse>
                {
                    Success = false,
                    Message = "Update failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<PromotionResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }
    }
}
