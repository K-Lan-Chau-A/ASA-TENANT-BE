using ASA_TENANT_SERVICE.DTOs.Common;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Interface
{
    public interface IVoucherService
    {
        Task<PagedResponse<VoucherResponse>> GetFilteredVouchersAsync(VoucherGetRequest Filter, int page, int pageSize);
        Task<ApiResponse<VoucherResponse>> CreateAsync(VoucherRequest request);
        Task<ApiResponse<VoucherResponse>> UpdateAsync(long id, VoucherRequest request);
        Task<ApiResponse<bool>> DeleteAsync(long id);
    }
}


