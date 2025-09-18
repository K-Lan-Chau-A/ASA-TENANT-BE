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
    public class ShopService : IShopService
    {
        private readonly ShopRepo _shopRepo;
        private readonly IMapper _mapper;
        public ShopService(ShopRepo shopRepo, IMapper mapper)
        {
            _shopRepo = shopRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse<ShopResponse>> CreateAsync(ShopRequest request)
        {
            try
            {
                var entity = _mapper.Map<Shop>(request);

                entity.CreatedAt = DateTime.UtcNow;

                var affected = await _shopRepo.CreateAsync(entity);

                if (affected > 0)
                {
                    var response = _mapper.Map<ShopResponse>(entity);
                    return new ApiResponse<ShopResponse>
                    {
                        Success = true,
                        Message = "Create successfully",
                        Data = response
                    };
                }

                return new ApiResponse<ShopResponse>
                {
                    Success = false,
                    Message = "Create failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ShopResponse>
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
                var existing = await _shopRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Shop not found",
                        Data = false
                    };

                var affected = await _shopRepo.RemoveAsync(existing);
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

        public async Task<PagedResponse<ShopResponse>> GetFilteredShopsAsync(ShopGetRequest Filter, int page, int pageSize)
        {
            var filter = _mapper.Map<Shop>(Filter);
            var query = _shopRepo.GetFiltered(filter);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResponse<ShopResponse>
            {
                Items = _mapper.Map<IEnumerable<ShopResponse>>(items),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<ShopResponse>> UpdateAsync(long id, ShopRequest request)
        {
            try
            {
                var existing = await _shopRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<ShopResponse>
                    {
                        Success = false,
                        Message = "Shop not found",
                        Data = null
                    };

                _mapper.Map(request, existing);

                var affected = await _shopRepo.UpdateAsync(existing);
                if (affected > 0)
                {
                    var response = _mapper.Map<ShopResponse>(existing);
                    return new ApiResponse<ShopResponse>
                    {
                        Success = true,
                        Message = "Update successfully",
                        Data = response
                    };
                }

                return new ApiResponse<ShopResponse>
                {
                    Success = false,
                    Message = "Update failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ShopResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<ShopResponse>> UpdateSepayApiKeyAsync(long id, string apiKey)
        {
            try
            {
                // Sử dụng method riêng chỉ cập nhật SepayApiKey
                var affected = await _shopRepo.UpdateSepayApiKeyAsync(id, apiKey);
                
                if (affected > 0)
                {
                    // Lấy lại shop sau khi cập nhật
                    var updatedShop = await _shopRepo.GetByIdAsync(id);
                    var response = _mapper.Map<ShopResponse>(updatedShop);
                    return new ApiResponse<ShopResponse>
                    {
                        Success = true,
                        Message = "Sepay API key updated successfully",
                        Data = response
                    };
                }

                return new ApiResponse<ShopResponse>
                {
                    Success = false,
                    Message = "Shop not found or failed to update Sepay API key",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ShopResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<ShopResponse>> TestSepayApiKeyAsync(string apiKey)
        {
            try
            {
                var shops = await _shopRepo.GetAllAsync();
                var shop = shops.FirstOrDefault(s => s.SepayApiKey == apiKey);

                if (shop != null)
                {
                    var response = _mapper.Map<ShopResponse>(shop);
                    return new ApiResponse<ShopResponse>
                    {
                        Success = true,
                        Message = "API key is valid",
                        Data = response
                    };
                }

                return new ApiResponse<ShopResponse>
                {
                    Success = false,
                    Message = "Invalid API key",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ShopResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<ShopResponse>> UpdateBankInfoAsync(long id, string bankName, string bankCode, string bankNum)
        {
            try
            {
                var existing = await _shopRepo.GetByIdAsync(id);
                if (existing == null)
                {
                    return new ApiResponse<ShopResponse>
                    {
                        Success = false,
                        Message = "Shop not found",
                        Data = null
                    };
                }

                existing.BankName = bankName;
                existing.BankCode = bankCode;
                existing.BankNum = bankNum;

                var affected = await _shopRepo.UpdateAsync(existing);
                if (affected > 0)
                {
                    var response = _mapper.Map<ShopResponse>(existing);
                    return new ApiResponse<ShopResponse>
                    {
                        Success = true,
                        Message = "Bank info updated successfully",
                        Data = response
                    };
                }

                return new ApiResponse<ShopResponse>
                {
                    Success = false,
                    Message = "Failed to update bank info",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ShopResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }
    }
}
