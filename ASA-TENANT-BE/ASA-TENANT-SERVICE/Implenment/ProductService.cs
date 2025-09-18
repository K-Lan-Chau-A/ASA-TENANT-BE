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
    public class ProductService : IProductService
    {
        private readonly ProductRepo _productRepo;
        private readonly UnitRepo _unitRepo;
        private readonly ProductUnitRepo _productUnitRepo;
        private readonly InventoryTransactionRepo _inventoryTransactionRepo;
        private readonly CategoryRepo _categoryRepo;
        private readonly IMapper _mapper;
        public ProductService(ProductRepo productRepo, IMapper mapper, UnitRepo unitRepo, ProductUnitRepo productUnitRepo, InventoryTransactionRepo inventoryTransactionRepo,CategoryRepo categoryRepo)
        {
            _productRepo = productRepo;
            _mapper = mapper;
            _unitRepo = unitRepo;
            _productUnitRepo = productUnitRepo;
            _categoryRepo = categoryRepo;
            _inventoryTransactionRepo = inventoryTransactionRepo;
        }

        
        public async Task<ApiResponse<ProductResponse>> CreateAsync(ProductRequest request)
        {
            try
            {
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
                var product = await _productRepo.GetByBarcodeAsync(request.Barcode, request.ShopId);

                if (product == null)
                {
                    product = await CreateNewProductAsync(request);
                }
                else
                {
                    product = await UpdateExistingProductAsync(product, request);
                }

                var response = _mapper.Map<ProductResponse>(product);
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

        private async Task<Product> CreateNewProductAsync(ProductRequest request)
        {
            var product = _mapper.Map<Product>(request);
            product.Quantity = request.InventoryTransaction.Quantity;
            product.IsLow = false;
            await _productRepo.CreateAsync(product);

            // Thêm đơn vị sản phẩm
            foreach (var unitReq in request.Units)
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

                var invTransaction = new InventoryTransaction
                {
                    ProductId = product.ProductId,
                    ShopId = product.ShopId,
                    UnitId = product.UnitIdFk,
                    Quantity = request.InventoryTransaction.Quantity,
                    Price = request.InventoryTransaction.Price,
                    ImageUrl = request.InventoryTransaction.ImageUrl,
                    CreatedAt = DateTime.UtcNow,
                    Type = 1 // nhập kho
                };

                await _inventoryTransactionRepo.CreateAsync(invTransaction);
            }

            await _productRepo.UpdateAsync(product);
            return product;
        }

        private async Task<Product> UpdateExistingProductAsync(Product product, ProductRequest request)
        {
            // Cập nhật số lượng tồn kho
            if (request.InventoryTransaction != null)
            {
                product.Quantity = (product.Quantity ?? 0) + request.InventoryTransaction.Quantity;
                product.Cost = request.InventoryTransaction.Price / request.InventoryTransaction.Quantity;
                product.Price = request.Price ?? product.Price;
                product.Discount = request.Discount ?? product.Discount;
                product.UpdateAt = DateTime.UtcNow;

                var invTransaction = new InventoryTransaction
                {
                    ProductId = product.ProductId,
                    ShopId = product.ShopId,
                    UnitId = product.UnitIdFk,
                    Quantity = request.InventoryTransaction.Quantity,
                    Price = request.InventoryTransaction.Price,
                    ImageUrl = request.InventoryTransaction.ImageUrl,
                    CreatedAt = DateTime.UtcNow,
                    Type = 1
                };

                await _inventoryTransactionRepo.CreateAsync(invTransaction);
            }

            // Update hoặc thêm mới đơn vị sản phẩm
            foreach (var unitReq in request.Units)
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


        public async Task<ApiResponse<bool>> DeleteAsync(long id)
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

                var affected = await _productRepo.RemoveAsync(existing);
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

        public async Task<PagedResponse<ProductResponse>> GetFilteredProductsAsync(ProductGetRequest Filter, int page, int pageSize)
        {
            var filter = _mapper.Map<Product>(Filter);
            var query = _productRepo.GetFiltered(filter);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResponse<ProductResponse>
            {
                Items = _mapper.Map<IEnumerable<ProductResponse>>(items),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<ProductResponse>> UpdateAsync(long id, ProductRequest request)
        {
            try
            {
                var existing = await _productRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<ProductResponse>
                    {
                        Success = false,
                        Message = "Product not found",
                        Data = null
                    };

                // Map dữ liệu từ DTO sang entity, bỏ Id
                _mapper.Map(request, existing);
                existing.UpdateAt = DateTime.UtcNow;

                var affected = await _productRepo.UpdateAsync(existing);
                if (affected > 0)
                {
                    var response = _mapper.Map<ProductResponse>(existing);
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
    }
}
