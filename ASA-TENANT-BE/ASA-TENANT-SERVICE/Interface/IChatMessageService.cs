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
    public interface IChatMessageService
    {
        Task<PagedResponse<ChatMessageResponse>> GetFilteredChatMessageAsync(ChatMessageGetRequest requestDto, int page, int pageSize);
        Task<ApiResponse<ChatMessageResponse>> CreateAsync(ChatMessageRequest request);
        Task<ApiResponse<ChatMessageResponse>> UpdateAsync(long id,ChatMessageRequest request);
        Task<ApiResponse<bool>> DeleteAsync(long id);
    }
}
