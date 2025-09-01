﻿using ASA_TENANT_SERVICE.DTOs.Common;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Interface
{
    public interface IPromotionService
    {
        Task<PagedResponse<PromotionResponse>> GetFilteredPromotionsAsync(PromotionGetRequest Filter, int page, int pageSize);
        Task<ApiResponse<PromotionResponse>> CreateAsync(PromotionRequest request);
        Task<ApiResponse<PromotionResponse>> UpdateAsync(long id, PromotionRequest request);
        Task<ApiResponse<bool>> DeleteAsync(long id);
    }
}
