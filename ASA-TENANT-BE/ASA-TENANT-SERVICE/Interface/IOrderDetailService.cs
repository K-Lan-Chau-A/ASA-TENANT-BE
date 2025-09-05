using ASA_TENANT_SERVICE.DTOs.Common;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Interface
{
    public interface IOrderDetailService
    {
        Task<PagedResponse<OrderDetailResponse>> GetFilteredOrderDetailsAsync(OrderDetailGetRequest Filter, int page, int pageSize);
        Task<ApiResponse<OrderDetailResponse>> CreateAsync(OrderDetailRequest request);
        Task<ApiResponse<OrderDetailResponse>> UpdateAsync(long id, OrderDetailRequest request);
        Task<ApiResponse<bool>> DeleteAsync(long id);
    }
}
