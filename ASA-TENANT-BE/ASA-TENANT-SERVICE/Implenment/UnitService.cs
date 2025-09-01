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
    public class UnitService : IUnitService
    {
        private readonly UnitRepo _unitRepo;
        private readonly IMapper _mapper;
        public UnitService(UnitRepo unitRepo, IMapper mapper)
        {
            _unitRepo = unitRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse<UnitResponse>> CreateAsync(UnitRequest request)
        {
            try
            {
                var entity = _mapper.Map<Unit>(request);

                var affected = await _unitRepo.CreateAsync(entity);

                if (affected > 0)
                {
                    var response = _mapper.Map<UnitResponse>(entity);
                    return new ApiResponse<UnitResponse>
                    {
                        Success = true,
                        Message = "Create successfully",
                        Data = response
                    };
                }

                return new ApiResponse<UnitResponse>
                {
                    Success = false,
                    Message = "Create failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<UnitResponse>
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
                var existing = await _unitRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unit not found",
                        Data = false
                    };

                var affected = await _unitRepo.RemoveAsync(existing);
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

        public async Task<PagedResponse<UnitResponse>> GetFilteredUnitsAsync(UnitGetRequest Filter, int page, int pageSize)
        {
            var filter = _mapper.Map<Unit>(Filter);
            var query = _unitRepo.GetFiltered(filter);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResponse<UnitResponse>
            {
                Items = _mapper.Map<IEnumerable<UnitResponse>>(items),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<UnitResponse>> UpdateAsync(long id, UnitRequest request)
        {
            try
            {
                var existing = await _unitRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<UnitResponse>
                    {
                        Success = false,
                        Message = "Unit not found",
                        Data = null
                    };

                _mapper.Map(request, existing);

                var affected = await _unitRepo.UpdateAsync(existing);
                if (affected > 0)
                {
                    var response = _mapper.Map<UnitResponse>(existing);
                    return new ApiResponse<UnitResponse>
                    {
                        Success = true,
                        Message = "Update successfully",
                        Data = response
                    };
                }

                return new ApiResponse<UnitResponse>
                {
                    Success = false,
                    Message = "Update failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<UnitResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }
    }
}
