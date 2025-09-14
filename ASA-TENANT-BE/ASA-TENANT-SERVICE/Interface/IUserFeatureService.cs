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
    public interface IUserFeatureService
    {
        Task<PagedResponse<UserFeatureResponse>> GetFilteredUsersFeatureAsync(UserFeatureGetRequest Filter, int page, int pageSize);
        Task<ApiResponse<List<UserFeatureResponse>>> CreateAsync(UserFeatureRequest request);
        Task<ApiResponse<List<UserFeatureResponse>>> UpdateAsync(UserFeatureUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(long id);
    }
}
