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
        private readonly IMapper _mapper;
        public PromotionService(PromotionRepo promotionRepo, IMapper mapper, PromotionProductRepo promotionProductRepo)
        {
            _promotionRepo = promotionRepo;
            _mapper = mapper;
            _promotionProductRepo = promotionProductRepo;
        }

        public async Task<ApiResponse<PromotionResponse>> CreateAsync(PromotionRequest request)
        {
            try
            {
                if (request.ProductIds != null && request.ProductIds.Any())
                {
                    var invalidIds = await _promotionRepo.GetInvalidProductIdsAsync(request.ProductIds);
                    if (invalidIds.Any())
                    {
                        return new ApiResponse<PromotionResponse>
                        {
                            Success = false,
                            Message = $"Invalid product id(s): {string.Join(", ", invalidIds)}",
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

                    // nếu có danh sách product
                    if (request.ProductIds != null && request.ProductIds.Any())
                    {
                        foreach (var productId in request.ProductIds)
                        {
                            var pp = new PromotionProduct
                            {
                                PromotionId = entity.PromotionId,
                                ProductId = productId
                            };

                            await _promotionProductRepo.CreateAsync(pp);
                            promotionProducts.Add(pp);
                        }
                    }

                    // map sang response
                    var response = _mapper.Map<PromotionResponse>(entity);

                    // set danh sách product response trả về
                    if (promotionProducts.Any())
                    {
                        response.Products = promotionProducts
                            .Select(pp => pp.ProductId.Value)
                            .ToHashSet();
                    }

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

                // validate product ids
                if (request.ProductIds != null && request.ProductIds.Any())
                {
                    var invalidIds = await _promotionRepo.GetInvalidProductIdsAsync(request.ProductIds);
                    if (invalidIds.Any())
                    {
                        return new ApiResponse<PromotionResponse>
                        {
                            Success = false,
                            Message = $"Invalid product id(s): {string.Join(", ", invalidIds)}",
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
                    if (request.ProductIds != null && request.ProductIds.Any())
                    {
                        foreach (var productId in request.ProductIds)
                        {
                            var pp = new PromotionProduct
                            {
                                PromotionId = existing.PromotionId,
                                ProductId = productId
                            };

                            await _promotionProductRepo.CreateAsync(pp);
                            promotionProducts.Add(pp);
                        }
                    }

                    // map sang response
                    var response = _mapper.Map<PromotionResponse>(existing);
                    if (promotionProducts.Any())
                    {
                        response.Products = promotionProducts
                            .Select(pp => pp.ProductId.Value)
                            .ToHashSet();
                    }

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
