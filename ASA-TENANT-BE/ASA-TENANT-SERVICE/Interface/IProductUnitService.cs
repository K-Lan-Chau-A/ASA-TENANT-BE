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
    public interface IProductUnitService
    {
        Task<PagedResponse<ProductUnitResponse>> GetFilteredProductUnitsAsync(ProductUnitGetRequest Filter, int page, int pageSize);
        Task<ApiResponse<ProductUnitResponse>> CreateAsync(ProductUnitRequest request);
        Task<ApiResponse<ProductUnitResponse>> UpdateAsync(long id, ProductUnitRequest request);
        Task<ApiResponse<bool>> DeleteAsync(long id);
    }
}
