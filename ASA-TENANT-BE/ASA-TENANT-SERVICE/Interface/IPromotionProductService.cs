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
    public interface IPromotionProductService
    {
        Task<PagedResponse<PromotionProductResponse>> GetFilteredPromotionProductsAsync(PromotionProductGetRequest Filter, int page, int pageSize);
        Task<ApiResponse<PromotionProductResponse>> CreateAsync(PromotionProductRequest request);
        Task<ApiResponse<PromotionProductResponse>> UpdateAsync(long id, PromotionProductRequest request);
        Task<ApiResponse<bool>> DeleteAsync(long id);
    }
}
