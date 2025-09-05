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
    public interface IFcmService
    {
        Task<PagedResponse<FcmResponse>> GetFilteredFcmAsync(FcmGetRequest requestDto, int page, int pageSize);
        Task<ApiResponse<FcmResponse>> CreateAsync(FcmRequest request);
        Task<ApiResponse<FcmResponse>> UpdateAsync(long id, FcmRequest request);
        Task<ApiResponse<bool>> DeleteAsync(long id);
    }
}
