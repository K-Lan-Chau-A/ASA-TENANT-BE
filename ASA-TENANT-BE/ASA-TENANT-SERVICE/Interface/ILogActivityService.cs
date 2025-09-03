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
    public interface ILogActivityService
    {
        Task<PagedResponse<LogActivityResponse>> GetFilteredLogActivitiesAsync(LogActivityGetRequest Filter, int page, int pageSize);
        Task<ApiResponse<LogActivityResponse>> CreateAsync(LogActivityRequest request);
        Task<ApiResponse<LogActivityResponse>> UpdateAsync(long id, LogActivityRequest request);
        Task<ApiResponse<bool>> DeleteAsync(long id);
    }
}
