using ASA_TENANT_SERVICE.DTOs.Common;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Interface
{
    public interface IOrderService
    {
        Task<PagedResponse<OrderResponse>> GetFilteredOrdersAsync(OrderGetRequest Filter, int page, int pageSize);
        Task<ApiResponse<OrderResponse>> GetByIdAsync(long id);
        Task<ApiResponse<OrderResponse>> GetByNoteAsync(string note);
        Task<ApiResponse<OrderResponse>> CreateAsync(OrderRequest request);
        Task<ApiResponse<OrderResponse>> UpdateAsync(long id, OrderRequest request);
        Task<ApiResponse<OrderResponse>> UpdateStatusAsync(long id, short status);
        Task<ApiResponse<bool>> DeleteAsync(long id);
    }
}
