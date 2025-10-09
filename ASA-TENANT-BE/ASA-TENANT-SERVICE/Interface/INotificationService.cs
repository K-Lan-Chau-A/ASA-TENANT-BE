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
    public interface INotificationService
    {
        Task<PagedResponse<NotificationResponse>> GetFilteredCategoriesAsync(NotificationGetRequest Filter, int page, int pageSize);
        Task<ApiResponse<NotificationResponse>> CreateAsync(NotificationRequest request);
        Task<ApiResponse<NotificationResponse>> UpdateAsync(long id, NotificationRequest request);
        Task<ApiResponse<bool>> DeleteAsync(long id);
        Task<ApiResponse<bool>> MarkAsReadAsync(long id);
        Task<ApiResponse<int>> MarkAllAsReadByUserAsync(long userId);
    }
}
