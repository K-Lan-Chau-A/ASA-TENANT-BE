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
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Implenment
{
    public class VoucherService : IVoucherService
    {
        private readonly VoucherRepo _voucherRepo;
        private readonly IMapper _mapper;
        public VoucherService(VoucherRepo voucherRepo, IMapper mapper)
        {
            _voucherRepo = voucherRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse<VoucherResponse>> CreateAsync(VoucherRequest request)
        {
            try
            {
                // Validation: Type 2 => Value in [1,100]; Type 1 => Value > 0
                if (request.Type.HasValue)
                {
                    if (request.Type.Value == 2)
                    {
                        if (!request.Value.HasValue || request.Value.Value < 1 || request.Value.Value > 100)
                        {
                            return new ApiResponse<VoucherResponse>
                            {
                                Success = false,
                                Message = "For type=2, value must be between 1 and 100",
                                Data = null
                            };
                        }
                    }
                    else if (request.Type.Value == 1)
                    {
                        if (!request.Value.HasValue || request.Value.Value <= 0)
                        {
                            return new ApiResponse<VoucherResponse>
                            {
                                Success = false,
                                Message = "For type=1, value must be greater than 0",
                                Data = null
                            };
                        }
                    }
                }
                var entity = _mapper.Map<Voucher>(request);
                var affected = await _voucherRepo.CreateAsync(entity);
                if (affected > 0)
                {
                    var response = _mapper.Map<VoucherResponse>(entity);
                    return new ApiResponse<VoucherResponse>
                    {
                        Success = true,
                        Message = "Create successfully",
                        Data = response
                    };
                }
                return new ApiResponse<VoucherResponse>
                {
                    Success = false,
                    Message = "Create failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<VoucherResponse>
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
                var existing = await _voucherRepo.GetByIdAsync(id);
                if (existing == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Voucher not found",
                        Data = false
                    };
                }
                var affected = await _voucherRepo.RemoveAsync(existing);
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

        public async Task<PagedResponse<VoucherResponse>> GetFilteredVouchersAsync(VoucherGetRequest Filter, int page, int pageSize)
        {
            var filter = _mapper.Map<Voucher>(Filter);
            var query = _voucherRepo.GetFiltered(filter);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResponse<VoucherResponse>
            {
                Items = _mapper.Map<IEnumerable<VoucherResponse>>(items),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<VoucherResponse>> UpdateAsync(long id, VoucherRequest request)
        {
            try
            {
                var existing = await _voucherRepo.GetByIdAsync(id);
                if (existing == null)
                {
                    return new ApiResponse<VoucherResponse>
                    {
                        Success = false,
                        Message = "Voucher not found",
                        Data = null
                    };
                }
                // Validation: Type 2 => Value in [1,100]; Type 1 => Value > 0
                if (request.Type.HasValue)
                {
                    if (request.Type.Value == 2)
                    {
                        if (!request.Value.HasValue || request.Value.Value < 1 || request.Value.Value > 100)
                        {
                            return new ApiResponse<VoucherResponse>
                            {
                                Success = false,
                                Message = "For type=2, value must be between 1 and 100",
                                Data = null
                            };
                        }
                    }
                    else if (request.Type.Value == 1)
                    {
                        if (!request.Value.HasValue || request.Value.Value <= 0)
                        {
                            return new ApiResponse<VoucherResponse>
                            {
                                Success = false,
                                Message = "For type=1, value must be greater than 0",
                                Data = null
                            };
                        }
                    }
                }
                _mapper.Map(request, existing);
                var affected = await _voucherRepo.UpdateAsync(existing);
                if (affected > 0)
                {
                    var response = _mapper.Map<VoucherResponse>(existing);
                    return new ApiResponse<VoucherResponse>
                    {
                        Success = true,
                        Message = "Update successfully",
                        Data = response
                    };
                }
                return new ApiResponse<VoucherResponse>
                {
                    Success = false,
                    Message = "Update failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<VoucherResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }
    }
}


