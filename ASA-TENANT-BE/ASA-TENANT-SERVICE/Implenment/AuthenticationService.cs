using ASA_TENANT_SERVICE.DTOs.Common;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Interface;
using ASA_TENANT_SERVICE.Enums;
using AutoMapper;
using ASA_TENANT_REPO.Repository;
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
        private readonly UserRepo _userRepo;
        public AuthenticationService(IUserService userService, IMapper mapper, UserRepo userRepo)
        {
            _userService = userService;
            _mapper = mapper;
            _userRepo = userRepo;
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
                    
                    // Kiểm tra UserStatus - không cho phép login nếu status = 0 (Inactive)
                    if (user.Status == (short)UserStatus.Inactive)
                    {
                        return new ApiResponse<LoginResponse>
                        {
                            Success = false,
                            Message = "Tài khoản đã bị ngưng hoạt động",
                            Data = null
                        };
                    }
                    
                    var shopId = user.ShopId.Value;
                    var response = _mapper.Map<LoginResponse>(user);
                    response.FeatureIds = await _userService.GetUserFeaturesList(user.UserId);
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

        public async Task<ApiResponse<bool>> ChangePassword(ChangePasswordRequest changePasswordRequest)
        {
            try
            {
                if (changePasswordRequest == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Request không hợp lệ",
                        Data = false
                    };
                }

                // Kiểm tra mật khẩu mới và xác nhận mật khẩu phải khớp
                if (changePasswordRequest.NewPassword != changePasswordRequest.ConfirmPassword)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Mật khẩu nhập lại không khớp",
                        Data = false
                    };
                }

                // Lấy thông tin user
                var user = await _userService.GetUserbyUserId(changePasswordRequest.UserId);
                if (user == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng",
                        Data = false
                    };
                }

                // Kiểm tra mật khẩu cũ có đúng không
                var isOldPasswordValid = BCrypt.Net.BCrypt.Verify(changePasswordRequest.OldPassword, user.Password);
                if (!isOldPasswordValid)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Mật khẩu cũ không đúng",
                        Data = false
                    };
                }

                // Hash mật khẩu mới và cập nhật
                user.Password = _userService.HashPassword(changePasswordRequest.NewPassword);
                var affected = await _userRepo.UpdateAsync(user);

                if (affected > 0)
                {
                    return new ApiResponse<bool>
                    {
                        Success = true,
                        Message = "Đổi mật khẩu thành công",
                        Data = true
                    };
                }

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Đổi mật khẩu thất bại",
                    Data = false
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = false
                };
            }
        }
    }
}
