using ASA_TENANT_SERVICE.DTOs.Common;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Interface;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Implenment
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        public AuthenticationService(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }
        public async Task<ApiResponse<LoginResponse>> CheckLogin(LoginRequest loginRequest)
        {
            try
            {
                if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Username) || string.IsNullOrEmpty(loginRequest.Password))
                {
                    return new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "Invalid login request",
                        Data = null
                    };
                }
                var user = await _userService.GetUserByUsername(loginRequest.Username);
                if (user != null)
                {
                    var isPasswordValid = BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.Password);
                    if (!isPasswordValid)
                    {
                        return new ApiResponse<LoginResponse>
                        {
                            Success = false,
                            Message = "Invalid Username or Password",
                            Data = null
                        };
                    }
                    var shopId = user.ShopId.Value;
                    var response = _mapper.Map<LoginResponse>(user);
                    return new ApiResponse<LoginResponse>
                    {
                        Success = true,
                        Message = "Login successful",
                        Data = response
                    };
                }
                return new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = "Invalid Username or Password",
                    Data = null
                };

            }
            catch (Exception ex)
            {
                return new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }
    }
}
