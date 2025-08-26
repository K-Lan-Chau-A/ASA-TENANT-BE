using ASA_TENANT_REPO.DBContext;
using ASA_TENANT_REPO.Models;
using ASA_TENANT_REPO.Repository;
using ASA_TENANT_SERVICE.DTOs.Common;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Interface;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Implenment
{
    public class CategoryService : ICategoryService
    {
        private readonly CategoryRepo _categoryRepo;
        private readonly IMapper _mapper;
        public CategoryService(CategoryRepo categoryRepo, IMapper mapper)
        {
            _categoryRepo = categoryRepo;
            _mapper = mapper;
        }
        public async Task<PagedResponse<CategoryResponse>> GetFilteredCategoriesAsync(CategoryGetRequest Filter, int page, int pageSize)
        {
            var filter = _mapper.Map<Category>(Filter);
            var query = _categoryRepo.GetFiltered(filter);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResponse<CategoryResponse>
            {
                Items = _mapper.Map<IEnumerable<CategoryResponse>>(items),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        public async Task<ApiResponse<CategoryResponse>> CreateAsync(CategoryRequest request)
        {
            try
            {
                var entity = _mapper.Map<Category>(request);

                var affected = await _categoryRepo.CreateAsync(entity);

                if (affected > 0)
                {
                    var response = _mapper.Map<CategoryResponse>(entity);
                    return new ApiResponse<CategoryResponse>
                    {
                        Success = true,
                        Message = "Create successfully",
                        Data = response
                    };
                }

                return new ApiResponse<CategoryResponse>
                {
                    Success = false,
                    Message = "Create failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<CategoryResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<CategoryResponse>> UpdateAsync(long id,CategoryRequest request)
        {
            try
            {
                var existing = await _categoryRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<CategoryResponse>
                    {
                        Success = false,
                        Message = "Category not found",
                        Data = null
                    };

                // Map dữ liệu từ DTO sang entity, bỏ Id
                _mapper.Map(request, existing);

                var affected = await _categoryRepo.UpdateAsync(existing);
                if (affected > 0)
                {
                    var response = _mapper.Map<CategoryResponse>(existing);
                    return new ApiResponse<CategoryResponse>
                    {
                        Success = true,
                        Message = "Update successfully",
                        Data = response
                    };
                }

                return new ApiResponse<CategoryResponse>
                {
                    Success = false,
                    Message = "Update failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<CategoryResponse>
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
                var existing = await _categoryRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Category not found",
                        Data = false
                    };

                var affected = await _categoryRepo.RemoveAsync(existing);
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
    }
}
