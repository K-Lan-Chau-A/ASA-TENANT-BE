using ASA_TENANT_SERVICE.DTOs.Common;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Interface
{
    public interface IProductService
    {
        Task<PagedResponse<ProductResponse>> GetFilteredProductsAsync(ProductGetRequest Filter, int page, int pageSize);
        Task<ApiResponse<ProductResponse>> CreateAsync(ProductRequest request);
        Task<ApiResponse<ProductResponse>> UpdateAsync(long id, ProductUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(long id, long shopid);
        Task<ApiResponse<bool>> ActivateAsync(long id, long shopid);
    }
}
