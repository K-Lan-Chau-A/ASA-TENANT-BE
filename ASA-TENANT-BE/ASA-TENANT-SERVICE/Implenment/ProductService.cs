using ASA_TENANT_REPO.DBContext;
using ASA_TENANT_REPO.Models;
using ASA_TENANT_REPO.Repository;
using ASA_TENANT_SERVICE.DTOs.Common;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Interface;
using ASA_TENANT_SERVICE.Enums;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace ASA_TENANT_SERVICE.Implenment
{
    public class ProductService : IProductService
    {
        private readonly ProductRepo _productRepo;
        private readonly UnitRepo _unitRepo;
        private readonly ProductUnitRepo _productUnitRepo;
        private readonly InventoryTransactionRepo _inventoryTransactionRepo;
        private readonly CategoryRepo _categoryRepo;
        private readonly PromotionProductRepo _promotionProductRepo;
        private readonly IPhotoService _photoService;
        private readonly IMapper _mapper;
        public ProductService(ProductRepo productRepo, IMapper mapper, UnitRepo unitRepo, ProductUnitRepo productUnitRepo, InventoryTransactionRepo inventoryTransactionRepo,CategoryRepo categoryRepo, PromotionProductRepo promotionProductRepo, IPhotoService photoService)
        {
            _productRepo = productRepo;
            _mapper = mapper;
            _unitRepo = unitRepo;
            _productUnitRepo = productUnitRepo;
            _categoryRepo = categoryRepo;
            _promotionProductRepo = promotionProductRepo;
            _photoService = photoService;
            _inventoryTransactionRepo = inventoryTransactionRepo;
        }

        
        public async Task<ApiResponse<ProductResponse>> CreateAsync(ProductRequest request)
        {
            try
            {
                var units = DeserializeUnitsJson(request.UnitsJson);
                if (request.CategoryId != null)
                {
                    var category = await _categoryRepo.GetByIdAndShopIdAsync(request.CategoryId.Value, request.ShopId);
                    if (category == null)
                    {
                        return new ApiResponse<ProductResponse>
                        {
                            Success = false,
                            Message = $"Error: CategoryId {request.CategoryId.Value} không thuộc ShopId {request.ShopId}",
                            Data = null
                        };
                    }
                }
                if (units == null || units.Count == 0)
                {
                    return new ApiResponse<ProductResponse>
                    {
                        Success = false,
                        Message = "Error: UnitsJson rỗng hoặc không hợp lệ. Vui lòng gửi mảng JSON hợp lệ.",
                        Data = null
                    };
                }

                Product product = null;
                if (!string.IsNullOrWhiteSpace(request.Barcode))
                {
                    product = await _productRepo.GetByBarcodeAsync(request.Barcode, request.ShopId);
                }
                else if (!string.IsNullOrWhiteSpace(request.ProductName))
                {
                    product = await _productRepo.GetByNameAsync(request.ProductName, request.ShopId);
                }

                if (product == null)
                {
                    product = await CreateNewProductAsync(request, units);
                }
                else
                {
                    product = await UpdateExistingProductAsync(product, request, units);
                }

                var response = _mapper.Map<ProductResponse>(product);
                response.PromotionPrice = await CalculatePromotionPriceAsync(product);
                return new ApiResponse<ProductResponse>
                {
                    Success = true,
                    Message = "Create/Update product success",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ProductResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }

        private async Task<Product> CreateNewProductAsync(ProductRequest request, List<UnitProductRequest> units)
        {
            var product = _mapper.Map<Product>(request);

            if(request.ProductImageFile != null)
            {
                var imageUrl = await _photoService.UploadImageAsync(request.ProductImageFile);
                product.ImageUrl = imageUrl;
            }

            product.Quantity = request.InventoryTransaction.Quantity;
            product.IsLow = request.IsLow;
            product.IsLowStockNotified = false;
            await _productRepo.CreateAsync(product);

            // Thêm đơn vị sản phẩm
            foreach (var unitReq in units)
            {
                var unit = await _unitRepo.GetOrCreateAsync(unitReq.Name, request.ShopId);

                if (unitReq.IsBaseUnit)
                    product.UnitIdFk = unit.UnitId;

                var productUnit = new ProductUnit
                {
                    ProductId = product.ProductId,
                    UnitId = unit.UnitId,
                    ConversionFactor = unitReq.ConversionFactor,
                    Price = unitReq.Price,
                    ShopId = product.ShopId
                };

                await _productUnitRepo.CreateAsync(productUnit);
            }

            // Tính cost từ transaction
            if (request.InventoryTransaction != null)
            {
                product.Cost = request.InventoryTransaction.Price / request.InventoryTransaction.Quantity;
                product.Price = request.Price ?? product.Price;
                product.UpdateAt = DateTime.UtcNow;

                string invImageUrl = null;
                if (request.InventoryTransaction.InventoryTransImageFile != null)
                {
                    invImageUrl = await _photoService.UploadImageAsync(request.InventoryTransaction.InventoryTransImageFile);
                }

                var invTransaction = new InventoryTransaction
                {
                    ProductId = product.ProductId,
                    ShopId = product.ShopId,
                    UnitId = product.UnitIdFk,
                    Quantity = request.InventoryTransaction.Quantity,
                    Price = request.InventoryTransaction.Price,
                    ImageUrl = invImageUrl,
                    CreatedAt = DateTime.UtcNow,
                    Type = 2 // nhập kho
                };

                await _inventoryTransactionRepo.CreateAsync(invTransaction);
            }

            await _productRepo.UpdateAsync(product);
            return product;
        }

        private async Task<Product> UpdateExistingProductAsync(Product product, ProductRequest request, List<UnitProductRequest> units)
        {
            // Upload ảnh sản phẩm nếu có
            if (request.ProductImageFile != null)
            {
                var imageUrl = await _photoService.UploadImageAsync(request.ProductImageFile);
                product.ImageUrl = imageUrl;
            }
            // Cập nhật số lượng tồn kho
            if (request.InventoryTransaction != null)
            {
                product.Quantity = (product.Quantity ?? 0) + request.InventoryTransaction.Quantity;
                product.Cost = request.InventoryTransaction.Price / request.InventoryTransaction.Quantity;
                product.Price = request.Price ?? product.Price;
                product.Discount = request.Discount ?? product.Discount;
                product.UpdateAt = DateTime.UtcNow;

                var threshold = product.IsLow ?? 0;
                var currentQty = product.Quantity ?? 0;
                if (currentQty > threshold && (product.IsLowStockNotified ?? false))
                {
                    product.IsLowStockNotified = false;
                }

                string invImageUrl = null;
                if (request.InventoryTransaction?.InventoryTransImageFile != null)
                {
                    invImageUrl = await _photoService.UploadImageAsync(request.InventoryTransaction.InventoryTransImageFile);
                }

                var invTransaction = new InventoryTransaction
                {
                    ProductId = product.ProductId,
                    ShopId = product.ShopId,
                    UnitId = product.UnitIdFk,
                    Quantity = request.InventoryTransaction.Quantity,
                    Price = request.InventoryTransaction.Price,
                    ImageUrl = invImageUrl,
                    CreatedAt = DateTime.UtcNow,
                    Type = 2
                };

                await _inventoryTransactionRepo.CreateAsync(invTransaction);
            }

            // Update hoặc thêm mới đơn vị sản phẩm
            foreach (var unitReq in units)
            {
                var unit = await _unitRepo.GetOrCreateAsync(unitReq.Name, request.ShopId);

                if (unitReq.IsBaseUnit)
                    product.UnitIdFk = unit.UnitId;

                var productUnit = await _productUnitRepo.GetByProductAndUnitAsync(product.ProductId, unit.UnitId, request.ShopId);
                if (productUnit == null)
                {
                    productUnit = new ProductUnit
                    {
                        ProductId = product.ProductId,
                        UnitId = unit.UnitId,
                        ConversionFactor = unitReq.ConversionFactor,
                        Price = unitReq.Price,
                        ShopId = product.ShopId
                    };
                    await _productUnitRepo.CreateAsync(productUnit);
                }
                else
                {
                    productUnit.ConversionFactor = unitReq.ConversionFactor;
                    productUnit.Price = unitReq.Price;
                    await _productUnitRepo.UpdateAsync(productUnit);
                }
            }

            await _productRepo.UpdateAsync(product);
            return product;
        }


        public async Task<ApiResponse<bool>> DeleteAsync(long id,long shopid)
        {
            try
            {
                var existing = await _productRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Product not found",
                        Data = false
                    };

                var affected = await _productRepo.UnActiveProduct(id, shopid);
                return new ApiResponse<bool>
                {
                    Success = affected,
                    Message = affected ? "UnActive successfully" : "UnActive failed",
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

        public async Task<PagedResponse<ProductResponse>> GetFilteredProductsAsync(ProductGetRequest Filter, int page, int pageSize)
        {
            var filter = _mapper.Map<Product>(Filter);
            var query = _productRepo.GetFiltered(filter);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var responses = _mapper.Map<List<ProductResponse>>(items);
            for (int i = 0; i < items.Count; i++)
            {
                responses[i].PromotionPrice = await CalculatePromotionPriceAsync(items[i]);
            }

            return new PagedResponse<ProductResponse>
            {
                Items = responses,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<ProductResponse>> UpdateAsync(long id, ProductUpdateRequest request)
        {
            try
            {
                var units = DeserializeUnitsJson(request.UnitsJson);
                // Kiểm tra barcode có trùng trong shop không
                if (!string.IsNullOrEmpty(request.Barcode))
                {
                    var existingBarcode = await _productRepo.GetByBarcodeAsync(request.Barcode, request.ShopId);
                if (units == null || units.Count == 0)
                {
                    return new ApiResponse<ProductResponse>
                    {
                        Success = false,
                        Message = "Error: UnitsJson rỗng hoặc không hợp lệ. Vui lòng gửi mảng JSON hợp lệ.",
                        Data = null
                    };
                }

                    if (existingBarcode != null && existingBarcode.ProductId != id)
                    {
                        throw new Exception("Barcode đã tồn tại trong shop này.");
                    }
                }
                var existing = await _productRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<ProductResponse>
                    {
                        Success = false,
                        Message = "Product not found",
                        Data = null
                    };

                if (existing.ShopId != request.ShopId)
                {
                    return new ApiResponse<ProductResponse>
                    {
                        Success = false,
                        Message = $"Error: Product {id} không thuộc ShopId {request.ShopId}",
                        Data = null
                    };
                }

                if (request.CategoryId.HasValue)
                {
                    var category = await _categoryRepo.GetByIdAndShopIdAsync(request.CategoryId.Value, request.ShopId);
                    if (category == null)
                    {
                        return new ApiResponse<ProductResponse>
                        {
                            Success = false,
                            Message = $"Error: CategoryId {request.CategoryId.Value} không tồn tại trong ShopId {request.ShopId}",
                            Data = null
                        };
                    }
                }

                // Map dữ liệu từ DTO sang entity, bỏ Id
                _mapper.Map(request, existing);
                existing.UpdateAt = DateTime.UtcNow;

                // Upload ảnh sản phẩm nếu có
                if (request.ProductImageFile != null)
                {
                    var imageUrl = await _photoService.UploadImageAsync(request.ProductImageFile);
                    existing.ImageUrl = imageUrl;
                }

                // Update hoặc thêm mới đơn vị sản phẩm
                foreach (var unitReq in units)
                {
                    if (string.IsNullOrWhiteSpace(unitReq.Name))
                        return new ApiResponse<ProductResponse>
                        {
                            Success = false,
                            Message = "Error: Unit name không được để trống",
                            Data = null
                        };
                    var unit = await _unitRepo.GetOrCreateAsync(unitReq.Name, request.ShopId);

                    if (unitReq.IsBaseUnit)
                        existing.UnitIdFk = unit.UnitId;

                    var productUnit = await _productUnitRepo.GetByProductAndUnitAsync(existing.ProductId, unit.UnitId, request.ShopId);
                    if (productUnit == null)
                    {
                        productUnit = new ProductUnit
                        {
                            ProductId = existing.ProductId,
                            UnitId = unit.UnitId,
                            ConversionFactor = unitReq.ConversionFactor,
                            Price = unitReq.Price,
                            ShopId = existing.ShopId
                        };
                        await _productUnitRepo.CreateAsync(productUnit);
                    }
                    else
                    {
                        productUnit.ConversionFactor = unitReq.ConversionFactor;
                        productUnit.Price = unitReq.Price;
                        await _productUnitRepo.UpdateAsync(productUnit);
                    }
                }

                var affected = await _productRepo.UpdateAsync(existing);
                if (affected > 0)
                {
                    var response = _mapper.Map<ProductResponse>(existing);
                    response.PromotionPrice = await CalculatePromotionPriceAsync(existing);
                    return new ApiResponse<ProductResponse>
                    {
                        Success = true,
                        Message = "Update successfully",
                        Data = response
                    };
                }

                return new ApiResponse<ProductResponse>
                {
                    Success = false,
                    Message = "Update failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ProductResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }

        private async Task<decimal?> CalculatePromotionPriceAsync(Product product)
        {
            if (product == null || product.Price == null || product.Price <= 0)
                return null;

            var filter = new PromotionProduct { ProductId = product.ProductId };
            var promosQuery = _promotionProductRepo.GetFiltered(filter)
                .Select(pp => pp.Promotion);

            var promos = await promosQuery.ToListAsync();
            if (promos == null || promos.Count == 0)
                return null;

            var now = DateTime.UtcNow;
            var today = DateOnly.FromDateTime(now);
            var currentTime = TimeOnly.FromDateTime(now);

            decimal basePrice = product.Price.Value;
            decimal currentPrice = basePrice;
            bool anyApplied = false;

            // Apply in deterministic order: Percentage first, then Money
            foreach (var promo in promos
                .Where(p => p != null)
                .OrderBy(p => p.Type == (short)PromotionType.Percentage ? 0 : 1))
            {
                if (promo.Status != (short?)PromotionStatus.Active)
                    continue;

                if (promo.ShopId != null && product.ShopId != null && promo.ShopId != product.ShopId)
                    continue;

                // Date window
                if (promo.StartDate != null && today < promo.StartDate.Value)
                    continue;
                if (promo.EndDate != null && today > promo.EndDate.Value)
                    continue;

                // Time window (if both set, enforce range within the day)
                if (promo.StartTime != null && promo.EndTime != null)
                {
                    if (currentTime < promo.StartTime.Value || currentTime > promo.EndTime.Value)
                        continue;
                }
                else if (promo.StartTime != null && currentTime < promo.StartTime.Value)
                {
                    continue;
                }
                else if (promo.EndTime != null && currentTime > promo.EndTime.Value)
                {
                    continue;
                }

                if (promo.Type == null || promo.Value == null)
                    continue;

                if (promo.Type == (short)PromotionType.Percentage)
                {
                    var pct = promo.Value.Value;
                    if (pct < 0) pct = 0;
                    if (pct > 100) pct = 100;
                    currentPrice = currentPrice - (currentPrice * (pct / 100m));
                    anyApplied = true;
                }
                else if (promo.Type == (short)PromotionType.Money)
                {
                    currentPrice = currentPrice - promo.Value.Value;
                    anyApplied = true;
                }

                if (currentPrice < 0)
                    currentPrice = 0;
            }

            if (!anyApplied)
                return null;

            return currentPrice;
        }

        private static List<UnitProductRequest> DeserializeUnitsJson(string unitsJson)
        {
            if (string.IsNullOrWhiteSpace(unitsJson))
            {
                return new List<UnitProductRequest>();
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var trimmed = unitsJson.Trim();

            try
            {
                // Case 1: JSON array
                if (trimmed.StartsWith("["))
                {
                    return JsonSerializer.Deserialize<List<UnitProductRequest>>(trimmed, options) ?? new List<UnitProductRequest>();
                }

                // Case 2: single object → wrap to list
                if (trimmed.StartsWith("{"))
                {
                    var single = JsonSerializer.Deserialize<UnitProductRequest>(trimmed, options);
                    return single != null ? new List<UnitProductRequest> { single } : new List<UnitProductRequest>();
                }
            }
            catch
            {
                // ignore and fall through to empty list
            }

            return new List<UnitProductRequest>();
        }
    }
}
