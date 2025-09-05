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
    public class PromptService : IPromptService
    {
        private readonly PromptRepo _promptRepo;
        private readonly IMapper _mapper;
        public PromptService(PromptRepo promptRepo, IMapper mapper)
        {
            _promptRepo = promptRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse<PromptResponse>> CreateAsync(PromptRequest request)
        {
            try
            {
                var entity = _mapper.Map<Prompt>(request);
                entity.CreatedAt = DateTime.UtcNow;

                var affected = await _promptRepo.CreateAsync(entity);

                if (affected > 0)
                {
                    var response = _mapper.Map<PromptResponse>(entity);
                    return new ApiResponse<PromptResponse>
                    {
                        Success = true,
                        Message = "Create successfully",
                        Data = response
                    };
                }

                return new ApiResponse<PromptResponse>
                {
                    Success = false,
                    Message = "Create failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<PromptResponse>
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
                var existing = await _promptRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Prompt not found",
                        Data = false
                    };

                var affected = await _promptRepo.RemoveAsync(existing);
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

        public async Task<PagedResponse<PromptResponse>> GetFilteredPromptsAsync(PromptGetRequest Filter, int page, int pageSize)
        {
            var filter = _mapper.Map<Prompt>(Filter);
            var query = _promptRepo.GetFiltered(filter);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResponse<PromptResponse>
            {
                Items = _mapper.Map<IEnumerable<PromptResponse>>(items),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<PromptResponse>> UpdateAsync(long id, PromptRequest request)
        {
            try
            {
                var existing = await _promptRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<PromptResponse>
                    {
                        Success = false,
                        Message = "Prompt not found",
                        Data = null
                    };

                _mapper.Map(request, existing);
                existing.UpdatedAt = DateTime.UtcNow;

                var affected = await _promptRepo.UpdateAsync(existing);
                if (affected > 0)
                {
                    var response = _mapper.Map<PromptResponse>(existing);
                    return new ApiResponse<PromptResponse>
                    {
                        Success = true,
                        Message = "Update successfully",
                        Data = response
                    };
                }

                return new ApiResponse<PromptResponse>
                {
                    Success = false,
                    Message = "Update failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<PromptResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }
    }
}
