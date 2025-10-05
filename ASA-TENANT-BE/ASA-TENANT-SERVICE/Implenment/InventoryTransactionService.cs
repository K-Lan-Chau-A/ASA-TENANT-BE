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
        private readonly ProductRepo _productRepo;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        public InventoryTransactionService(InventoryTransactionRepo inventoryTransactionRepo, ProductRepo productRepo, IMapper mapper, IPhotoService photoService)
        {
            _inventoryTransactionRepo = inventoryTransactionRepo;
            _productRepo = productRepo;
            _mapper = mapper;
            _photoService = photoService;
        }

        public async Task<ApiResponse<InventoryTransactionResponse>> CreateAsync(InventoryTransactionRequest request)
        {
            try
            {
                var entity = _mapper.Map<InventoryTransaction>(request);
                entity.CreatedAt = DateTime.UtcNow;
                if (request.InventoryTransImageFile != null)
                {
                    var imageUrl = await _photoService.UploadImageAsync(request.InventoryTransImageFile);
                    entity.ImageUrl = imageUrl;
                }
                var affected = await _inventoryTransactionRepo.CreateAsync(entity);

                if (affected > 0)
                {
                    if (entity.Type == 2 && entity.ProductId.HasValue)
                    {
                        var product = await _productRepo.GetByIdAsync(entity.ProductId.Value);
                        if (product != null)
                        {
                            var threshold = product.IsLow ?? 0;
                            var currentQty = product.Quantity ?? 0;
                            if (currentQty > threshold && (product.IsLowStockNotified ?? false))
                            {
                                product.IsLowStockNotified = false;
                                await _productRepo.UpdateAsync(product);
                            }
                        }
                    }
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


                var oldProductId = existing.ProductId;
                var oldQuantity = existing.Quantity ?? 0;

                _mapper.Map(request, existing);
                if (request.InventoryTransImageFile != null)
                {
                    var imageUrl = await _photoService.UploadImageAsync(request.InventoryTransImageFile);
                    existing.ImageUrl = imageUrl;
                }

                if (oldProductId == existing.ProductId)
                {
                    var product = await _productRepo.GetByIdAsync(existing.ProductId);
                    if (product != null)
                    {
                        var currentQty = product.Quantity ?? 0;
                        var delta = request.Quantity - oldQuantity;
                        product.Quantity = currentQty + delta;

                        if (request.Price.HasValue && request.Quantity > 0)
                        {
                            product.Cost = request.Price.Value / request.Quantity;
                        }
                        await _productRepo.UpdateAsync(product);

                        // Nếu là nhập kho (type = 2), kiểm tra và reset cờ khi vượt ngưỡng
                        if (existing.Type == 2)
                        {
                            var threshold = product.IsLow ?? 0;
                            var qty = product.Quantity ?? 0;
                            if (qty > threshold && (product.IsLowStockNotified ?? false))
                            {
                                product.IsLowStockNotified = false;
                                await _productRepo.UpdateAsync(product);
                            }
                        }
                    }
                }
                else
                {
                    if (oldProductId.HasValue)
                    {
                        var oldProduct = await _productRepo.GetByIdAsync(oldProductId.Value);
                        if (oldProduct != null)
                        {
                            var oldProdQty = oldProduct.Quantity ?? 0;
                            oldProduct.Quantity = oldProdQty - oldQuantity;
                            await _productRepo.UpdateAsync(oldProduct);
                        }
                    }

                    var newProduct = await _productRepo.GetByIdAsync(existing.ProductId);
                    if (newProduct != null)
                    {
                        var newProdQty = newProduct.Quantity ?? 0;
                        newProduct.Quantity = newProdQty + request.Quantity;
                        if (request.Price.HasValue && request.Quantity > 0)
                        {
                            newProduct.Cost = request.Price.Value / request.Quantity;
                        }
                        await _productRepo.UpdateAsync(newProduct);

                        if (existing.Type == 2)
                        {
                            var threshold = newProduct.IsLow ?? 0;
                            var qty = newProduct.Quantity ?? 0;
                            if (qty > threshold && (newProduct.IsLowStockNotified ?? false))
                            {
                                newProduct.IsLowStockNotified = false;
                                await _productRepo.UpdateAsync(newProduct);
                            }
                        }
                    }
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
