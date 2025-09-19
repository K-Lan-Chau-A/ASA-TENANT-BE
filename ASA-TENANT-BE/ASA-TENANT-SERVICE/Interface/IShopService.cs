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
    public interface IShopService
    {
        Task<PagedResponse<ShopResponse>> GetFilteredShopsAsync(ShopGetRequest Filter, int page, int pageSize);
        Task<ApiResponse<ShopResponse>> CreateAsync(ShopRequest request);
        Task<ApiResponse<ShopResponse>> UpdateAsync(long id, ShopRequest request);
        Task<ApiResponse<bool>> DeleteAsync(long id);
        Task<ApiResponse<ShopResponse>> UpdateSepayApiKeyAsync(long id, string apiKey);
        Task<ApiResponse<ShopResponse>> TestSepayApiKeyAsync(string apiKey);
        Task<ApiResponse<ShopResponse>> UpdateBankInfoAsync(long id, string bankName, string bankCode, string bankNum);
    }
}
