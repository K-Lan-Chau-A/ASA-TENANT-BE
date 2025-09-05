using ASA_TENANT_SERVICE.DTOs.Common;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Interface
{
    public interface ITransactionService
    {
        Task<PagedResponse<TransactionResponse>> GetFilteredTransactionsAsync(TransactionGetRequest Filter, int page, int pageSize);
        Task<ApiResponse<TransactionResponse>> CreateAsync(TransactionRequest request);
        Task<ApiResponse<TransactionResponse>> UpdateAsync(long id, TransactionRequest request);
        Task<ApiResponse<bool>> DeleteAsync(long id);
    }
}


