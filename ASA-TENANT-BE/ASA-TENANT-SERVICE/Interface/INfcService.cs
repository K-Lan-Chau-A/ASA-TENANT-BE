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
    public interface INfcService
    {
        Task<PagedResponse<NfcResponse>> GetFilteredNfcsAsync(NfcGetRequest Filter, int page, int pageSize);
        Task<ApiResponse<NfcResponse>> CreateAsync(NfcRequest request);
        Task<ApiResponse<NfcResponse>> UpdateAsync(long id, NfcRequest request);
        Task<ApiResponse<bool>> DeleteAsync(long id);
    }
}
