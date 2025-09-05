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
    public class ProductUnitService : IProductUnitService
    {
        private readonly ProductUnitRepo _productUnitRepo;
        private readonly IMapper _mapper;
        public ProductUnitService(ProductUnitRepo productUnitRepo, IMapper mapper)
        {
            _productUnitRepo = productUnitRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse<ProductUnitResponse>> CreateAsync(ProductUnitRequest request)
        {
            try
            {
                var entity = _mapper.Map<ProductUnit>(request);

                var affected = await _productUnitRepo.CreateAsync(entity);

                if (affected > 0)
                {
                    var response = _mapper.Map<ProductUnitResponse>(entity);
                    return new ApiResponse<ProductUnitResponse>
                    {
                        Success = true,
                        Message = "Create successfully",
                        Data = response
                    };
                }

                return new ApiResponse<ProductUnitResponse>
                {
                    Success = false,
                    Message = "Create failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ProductUnitResponse>
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
                var existing = await _productUnitRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Product Unit not found",
                        Data = false
                    };

                var affected = await _productUnitRepo.RemoveAsync(existing);
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

        public async Task<PagedResponse<ProductUnitResponse>> GetFilteredProductUnitsAsync(ProductUnitGetRequest Filter, int page, int pageSize)
        {
            var filter = _mapper.Map<ProductUnit>(Filter);
            var query = _productUnitRepo.GetFiltered(filter);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResponse<ProductUnitResponse>
            {
                Items = _mapper.Map<IEnumerable<ProductUnitResponse>>(items),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<ProductUnitResponse>> UpdateAsync(long id, ProductUnitRequest request)
        {
            try
            {
                var existing = await _productUnitRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<ProductUnitResponse>
                    {
                        Success = false,
                        Message = "Product Unit not found",
                        Data = null
                    };

                _mapper.Map(request, existing);

                var affected = await _productUnitRepo.UpdateAsync(existing);
                if (affected > 0)
                {
                    var response = _mapper.Map<ProductUnitResponse>(existing);
                    return new ApiResponse<ProductUnitResponse>
                    {
                        Success = true,
                        Message = "Update successfully",
                        Data = response
                    };
                }

                return new ApiResponse<ProductUnitResponse>
                {
                    Success = false,
                    Message = "Update failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ProductUnitResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }
    }
}
