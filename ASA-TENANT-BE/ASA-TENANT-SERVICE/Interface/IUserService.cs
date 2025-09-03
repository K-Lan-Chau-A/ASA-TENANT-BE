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
    public interface IUserService
    {
        Task<PagedResponse<UserResponse>> GetFilteredUsersAsync(UserGetRequest Filter, int page, int pageSize);
        Task<ApiResponse<UserResponse>> CreateAsync(UserRequest request);
        Task<ApiResponse<UserResponse>> UpdateAsync(long id, UserRequest request);
        Task<ApiResponse<bool>> DeleteAsync(long id);
    }
}
