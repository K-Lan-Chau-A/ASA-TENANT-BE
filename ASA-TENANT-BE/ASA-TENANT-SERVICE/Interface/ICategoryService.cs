using ASA_TENANT_REPO.Models;
using ASA_TENANT_SERVICE.DTOs.Common;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Interface
{
    public interface ICategoryService
    {
        Task<PagedResponse<CategoryResponse>> GetFilteredCategoriesAsync(CategoryGetRequest Filter , int page, int pageSize);
        Task<ApiResponse<CategoryResponse>> CreateAsync(CategoryRequest request);
        Task<ApiResponse<CategoryResponse>> UpdateAsync(long id ,CategoryRequest request);
        Task<ApiResponse<bool>> DeleteAsync(long id);
    }
}
