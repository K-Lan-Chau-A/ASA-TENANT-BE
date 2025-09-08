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
    public class NfcService : INfcService
    {
        private readonly NfcRepo _nfcRepo;
        private readonly IMapper _mapper;
        public NfcService(NfcRepo nfcRepo, IMapper mapper)
        {
            _nfcRepo = nfcRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse<NfcResponse>> CreateAsync(NfcRequest request)
        {
            try
            {
                var entity = _mapper.Map<Nfc>(request);
                entity.CreatedAt = DateTime.UtcNow;
                entity.LastUsedDate = DateTime.UtcNow;

                var affected = await _nfcRepo.CreateAsync(entity);
                

                if (affected > 0)
                {
                    var response = _mapper.Map<NfcResponse>(entity);
                    return new ApiResponse<NfcResponse>
                    {
                        Success = true,
                        Message = "Create successfully",
                        Data = response
                    };
                }

                return new ApiResponse<NfcResponse>
                {
                    Success = false,
                    Message = "Create failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<NfcResponse>
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
                var existing = await _nfcRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "NFC not found",
                        Data = false
                    };

                var affected = await _nfcRepo.RemoveAsync(existing);
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

        public async Task<PagedResponse<NfcResponse>> GetFilteredNfcsAsync(NfcGetRequest Filter, int page, int pageSize)
        {
            var filter = _mapper.Map<Nfc>(Filter);
            var query = _nfcRepo.GetFiltered(filter);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResponse<NfcResponse>
            {
                Items = _mapper.Map<IEnumerable<NfcResponse>>(items),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<NfcResponse>> UpdateAsync(long id, NfcRequest request)
        {
            try
            {
                var existing = await _nfcRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<NfcResponse>
                    {
                        Success = false,
                        Message = "NFC not found",
                        Data = null
                    };

                _mapper.Map(request, existing);

                var affected = await _nfcRepo.UpdateAsync(existing);
                if (affected > 0)
                {
                    var response = _mapper.Map<NfcResponse>(existing);
                    return new ApiResponse<NfcResponse>
                    {
                        Success = true,
                        Message = "Update successfully",
                        Data = response
                    };
                }

                return new ApiResponse<NfcResponse>
                {
                    Success = false,
                    Message = "Update failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<NfcResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }
    }
}
