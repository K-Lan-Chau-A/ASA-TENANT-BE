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
    public interface IShopSubscriptionService
    {
        Task<PagedResponse<ShopSubscriptionResponse>> GetFilteredAsync(ShopSubscriptionGetRequest filter, int page, int pageSize);
        Task<ApiResponse<ShopSubscriptionResponse>> CreateAsync(ShopSubscriptionRequest request);
        Task<ApiResponse<ShopSubscriptionResponse>> UpdateAsync(long id, ShopSubscriptionRequest request);
        Task<ApiResponse<bool>> DeleteAsync(long id);
    }
}
