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
    public interface IRankService
    {
        Task<PagedResponse<RankResponse>> GetFilteredUnitsAsync(RankGetRequest Filter, int page, int pageSize);
        Task<ApiResponse<RankResponse>> CreateAsync(RankRequest request);
        Task<ApiResponse<RankResponse>> UpdateAsync(int id, RankRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
