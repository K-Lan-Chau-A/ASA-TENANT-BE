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
        private readonly PromotionProductRepo _promotionProductRepo;
        private readonly ProductUnitRepo _productUnitRepo;
        private readonly IMapper _mapper;
        public PromotionService(PromotionRepo promotionRepo, IMapper mapper, PromotionProductRepo promotionProductRepo, ProductUnitRepo productUnitRepo)
        {
            _promotionRepo = promotionRepo;
            _mapper = mapper;
            _promotionProductRepo = promotionProductRepo;
            _productUnitRepo = productUnitRepo;
        }

        public async Task<ApiResponse<PromotionResponse>> CreateAsync(PromotionRequest request)
        {
            try
            {
                if (request.StartDate != null && request.EndDate != null && request.StartTime.Value >= request.EndTime.Value)
                {
                    return new ApiResponse<PromotionResponse>
                    {
                        Success = false,
                        Message = "StartTime must be less than EndTime",
                        Data = null
                    };
                }
                if (request.ProductUnitIds != null && request.ProductUnitIds.Any())
                {
                    // Validate product_unit ids exist and belong to shop
                    var validPuIds = await _productUnitRepo.GetFiltered(new ProductUnit { ShopId = request.ShopId })
                        .Where(pu => request.ProductUnitIds.Contains(pu.ProductUnitId))
                        .Select(pu => pu.ProductUnitId)
                        .ToListAsync();
                    var invalidPuIds = request.ProductUnitIds.Except(validPuIds).ToList();
                    if (invalidPuIds.Any())
                    {
                        return new ApiResponse<PromotionResponse>
                        {
                            Success = false,
                            Message = $"Invalid product_unit id(s): {string.Join(", ", invalidPuIds)}",
                            Data = null
                        };
                    }
                }
                
                // map request sang entity Promotion
                var entity = _mapper.Map<Promotion>(request);

                // tạo promotion
                var affected = await _promotionRepo.CreateAsync(entity);

                if (affected > 0)
                {
                    var promotionProducts = new List<PromotionProduct>();

                    // nếu FE truyền ProductUnitIds => tạo theo product_unit
                    if (request.ProductUnitIds != null && request.ProductUnitIds.Any())
                    {
                        var productUnits = await _productUnitRepo.GetFiltered(new ProductUnit { ShopId = request.ShopId })
                            .Where(pu => request.ProductUnitIds.Contains(pu.ProductUnitId))
                            .Select(pu => new { pu.ProductUnitId, pu.ProductId, pu.UnitId })
                            .ToListAsync();

                        foreach (var pu in productUnits)
                        {
                            var pp = new PromotionProduct
                            {
                                PromotionId = entity.PromotionId,
                                ProductId = pu.ProductId ?? 0,
                                UnitId = pu.UnitId ?? 0
                            };
                            await _promotionProductRepo.CreateAsync(pp);
                            promotionProducts.Add(pp);
                        }
                    }
                    

                    // map sang response
                    var response = _mapper.Map<PromotionResponse>(entity);

                    // giữ nguyên response, Products đã được loại bỏ

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

                // Xóa các PromotionProduct liên quan
                var promotionProducts = await _promotionProductRepo.GetByPromotionIdAsync(id);
                foreach (var pp in promotionProducts)
                {
                    await _promotionProductRepo.RemoveAsync(pp);
                }

                // Sau đó xóa promotion
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

                // Validate date range
                if (request.StartDate != null && request.EndDate != null && request.StartTime.Value >= request.EndTime.Value)
                {
                    return new ApiResponse<PromotionResponse>
                    {
                        Success = false,
                        Message = "StartTime must be less than EndTime",
                        Data = null
                    };
                }

                // validate product ids
                if (request.ProductUnitIds != null && request.ProductUnitIds.Any())
                {
                    var validPuIds = await _productUnitRepo.GetFiltered(new ProductUnit { ShopId = existing.ShopId ?? 0 })
                        .Where(pu => request.ProductUnitIds.Contains(pu.ProductUnitId))
                        .Select(pu => pu.ProductUnitId)
                        .ToListAsync();
                    var invalidPuIds = request.ProductUnitIds.Except(validPuIds).ToList();
                    if (invalidPuIds.Any())
                    {
                        return new ApiResponse<PromotionResponse>
                        {
                            Success = false,
                            Message = $"Invalid product_unit id(s): {string.Join(", ", invalidPuIds)}",
                            Data = null
                        };
                    }
                }
                

                // map data từ request sang entity
                _mapper.Map(request, existing);
                existing.StartDate = request.StartDate;
                existing.EndDate = request.EndDate;
                existing.StartTime = request.StartTime;
                existing.EndTime = request.EndTime;
                existing.Value = request.Value;
                existing.Type = (short)request.Type;
                existing.Status = request.Status;
                existing.Name = request.Name;
                existing.ShopId = request.ShopId;


                // update promotion
                var affected = await _promotionRepo.UpdateAsync(existing);
                if (affected > 0)
                {
                    var promotionProducts = new List<PromotionProduct>();

                    // Xóa product cũ
                    var oldProducts = await _promotionProductRepo.GetByPromotionIdAsync(id);
                    foreach (var op in oldProducts)
                    {
                        await _promotionProductRepo.RemoveAsync(op);
                    }

                // Thêm product mới
                if (request.ProductUnitIds != null && request.ProductUnitIds.Any())
                {
                    var productUnits = await _productUnitRepo.GetFiltered(new ProductUnit { ShopId = existing.ShopId ?? 0 })
                        .Where(pu => request.ProductUnitIds.Contains(pu.ProductUnitId))
                        .Select(pu => new { pu.ProductUnitId, pu.ProductId, pu.UnitId })
                        .ToListAsync();

                    foreach (var pu in productUnits)
                    {
                        var pp = new PromotionProduct
                        {
                            PromotionId = existing.PromotionId,
                            ProductId = pu.ProductId ?? 0,
                            UnitId = pu.UnitId ?? 0
                        };

                        await _promotionProductRepo.CreateAsync(pp);
                        promotionProducts.Add(pp);
                    }
                }
                

                    // map sang response
                    var response = _mapper.Map<PromotionResponse>(existing);
                    // giữ nguyên response, Products đã được loại bỏ

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
