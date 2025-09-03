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
    public interface IUnitService
    {
        Task<PagedResponse<UnitResponse>> GetFilteredUnitsAsync(UnitGetRequest Filter, int page, int pageSize);
        Task<ApiResponse<UnitResponse>> CreateAsync(UnitRequest request);
        Task<ApiResponse<UnitResponse>> UpdateAsync(long id, UnitRequest request);
        Task<ApiResponse<bool>> DeleteAsync(long id);
    }
}
