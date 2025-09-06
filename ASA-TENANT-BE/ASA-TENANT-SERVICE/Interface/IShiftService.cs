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
    public interface IShiftService
    {
        Task<PagedResponse<ShiftResponse>> GetFilteredCategoriesAsync(ShiftGetRequest Filter, int page, int pageSize);
        Task<ApiResponse<ShiftResponse>> CreateAsync(ShiftRequest request);
        Task<ApiResponse<ShiftResponse>> UpdateAsync(long id, ShiftRequest request);
        Task<ApiResponse<bool>> DeleteAsync(long id);
        Task<ApiResponse<ShiftResponse>> OpenShift(ShiftOpenRequest shiftOpenRequest);
        Task<ApiResponse<ShiftResponse>> CloseShift(ShiftCloseRequest shiftCloseRequest);
    }
}
