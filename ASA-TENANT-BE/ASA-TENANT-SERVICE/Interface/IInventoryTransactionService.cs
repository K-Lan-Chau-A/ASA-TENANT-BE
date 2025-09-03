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
    public interface IInventoryTransactionService
    {
        Task<PagedResponse<InventoryTransactionResponse>> GetFilteredInventoryTransactionsAsync(InventoryTransactionGetRequest Filter, int page, int pageSize);
        Task<ApiResponse<InventoryTransactionResponse>> CreateAsync(InventoryTransactionRequest request);
        Task<ApiResponse<InventoryTransactionResponse>> UpdateAsync(long id, InventoryTransactionRequest request);
        Task<ApiResponse<bool>> DeleteAsync(long id);
    }
}
