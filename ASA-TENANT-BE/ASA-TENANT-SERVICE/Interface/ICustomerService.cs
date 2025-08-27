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
    public interface ICustomerService
    {
        Task<PagedResponse<CustomerResponse>> GetFilteredCustomersAsync(CustomerGetRequest Filter, int page, int pageSize);
        Task<ApiResponse<CustomerResponse>> CreateAsync(CustomerRequest request);
        Task<ApiResponse<CustomerResponse>> UpdateAsync(long id, CustomerRequest request);
        Task<ApiResponse<bool>> DeleteAsync(long id);
    }
}
