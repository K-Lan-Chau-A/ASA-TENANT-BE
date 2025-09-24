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
    public class InventoryTransactionService : IInventoryTransactionService
    {
        private readonly InventoryTransactionRepo _inventoryTransactionRepo;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        public InventoryTransactionService(InventoryTransactionRepo inventoryTransactionRepo, IMapper mapper, IPhotoService photoService)
        {
            _inventoryTransactionRepo = inventoryTransactionRepo;
            _mapper = mapper;
            _photoService = photoService;
        }

        public async Task<ApiResponse<InventoryTransactionResponse>> CreateAsync(InventoryTransactionRequest request)
        {
            try
            {
                var entity = _mapper.Map<InventoryTransaction>(request);
                entity.CreatedAt = DateTime.UtcNow;
                if (request.ImageFile != null)
                {
                    var imageUrl = await _photoService.UploadImageAsync(request.ImageFile);
                    entity.ImageUrl = imageUrl;
                }
                var affected = await _inventoryTransactionRepo.CreateAsync(entity);

                if (affected > 0)
                {
                    var response = _mapper.Map<InventoryTransactionResponse>(entity);
                    return new ApiResponse<InventoryTransactionResponse>
                    {
                        Success = true,
                        Message = "Create successfully",
                        Data = response
                    };
                }

                return new ApiResponse<InventoryTransactionResponse>
                {
                    Success = false,
                    Message = "Create failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<InventoryTransactionResponse>
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
                var existing = await _inventoryTransactionRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Inventory Transaction not found",
                        Data = false
                    };

                var affected = await _inventoryTransactionRepo.RemoveAsync(existing);
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

        public async Task<PagedResponse<InventoryTransactionResponse>> GetFilteredInventoryTransactionsAsync(InventoryTransactionGetRequest Filter, int page, int pageSize)
        {
            var filter = _mapper.Map<InventoryTransaction>(Filter);
            var query = _inventoryTransactionRepo.GetFiltered(filter);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResponse<InventoryTransactionResponse>
            {
                Items = _mapper.Map<IEnumerable<InventoryTransactionResponse>>(items),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<InventoryTransactionResponse>> UpdateAsync(long id, InventoryTransactionRequest request)
        {
            try
            {
                var existing = await _inventoryTransactionRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<InventoryTransactionResponse>
                    {
                        Success = false,
                        Message = "Inventory Transaction not found",
                        Data = null
                    };

                _mapper.Map(request, existing);
                if (request.ImageFile != null)
                {
                    var imageUrl = await _photoService.UploadImageAsync(request.ImageFile);
                    existing.ImageUrl = imageUrl;
                }
                var affected = await _inventoryTransactionRepo.UpdateAsync(existing);
                if (affected > 0)
                {
                    var response = _mapper.Map<InventoryTransactionResponse>(existing);
                    return new ApiResponse<InventoryTransactionResponse>
                    {
                        Success = true,
                        Message = "Update successfully",
                        Data = response
                    };
                }

                return new ApiResponse<InventoryTransactionResponse>
                {
                    Success = false,
                    Message = "Update failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<InventoryTransactionResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }
    }
}
