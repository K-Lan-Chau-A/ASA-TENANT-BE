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
    public interface IPromptService
    {
        Task<PagedResponse<PromptResponse>> GetFilteredPromptsAsync(PromptGetRequest Filter, int page, int pageSize);
        Task<ApiResponse<PromptResponse>> CreateAsync(PromptRequest request);
        Task<ApiResponse<PromptResponse>> UpdateAsync(long id, PromptRequest request);
        Task<ApiResponse<bool>> DeleteAsync(long id);
    }
}
