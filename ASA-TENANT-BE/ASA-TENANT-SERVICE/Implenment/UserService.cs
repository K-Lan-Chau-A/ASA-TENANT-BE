using ASA_TENANT_REPO.DBContext;
using ASA_TENANT_REPO.Models;
using ASA_TENANT_REPO.Repository;
using ASA_TENANT_SERVICE.DTOs.Common;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Interface;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Implenment
{
    public class UserService : IUserService
    {
        private readonly UserRepo _userRepo;
        private readonly UserFeatureRepo _userFeatureRepo;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        public UserService(UserRepo userRepo, UserFeatureRepo userFeatureRepo, IMapper mapper, IPhotoService photoService)
        {
            _userRepo = userRepo;
            _userFeatureRepo = userFeatureRepo;
            _mapper = mapper;
            _photoService = photoService;
        }

        public async Task<ApiResponse<UserResponse>> CreateStaffAsync(UserCreateRequest request)
        {
            try
            {        
                var entity = _mapper.Map<User>(request);

                entity.CreatedAt = DateTime.UtcNow;
                entity.Status = 1; // Default active
                if (!string.IsNullOrWhiteSpace(request.Password))
                {
                    entity.Password = HashPassword(request.Password);
                }
                entity.Role = 2; // Default role is staff

                if (request.AvatarFile != null)
                {
                    var imageUrl = await _photoService.UploadImageAsync(request.AvatarFile);
                    entity.Avatar = imageUrl;
                }

                var affected = await _userRepo.CreateAsync(entity);

                if (affected > 0)
                {
                    // Tạo user features cho staff mới
                    await CreateUserFeaturesForStaff(entity.UserId, request.ShopId, request.FeatureIds);
                    
                    var response = _mapper.Map<UserResponse>(entity);
                    return new ApiResponse<UserResponse>
                    {
                        Success = true,
                        Message = "Create successfully",
                        Data = response
                    };
                }

                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = "Create failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }

        //public async Task<ApiResponse<UserAdminResponse>> CreateAdminAsync(UserAdminCreateRequest adminRequest)
        //{
        //    try
        //    {   // Check if admin user already exists
        //        var existingAdmin = await _userRepo.GetFirstUserAdmin(adminRequest.ShopId.Value);
        //        if (existingAdmin != null)
        //        {
        //            return new ApiResponse<UserAdminResponse>
        //            {
        //                Success = false,
        //                Message = "Only 1 admin user can exists in shop",
        //                Data = null
        //            };
        //        }
        //        var entity = _mapper.Map<User>(adminRequest);
        //        entity.CreatedAt = DateTime.UtcNow;
        //        entity.Password = HashPassword("123456");
        //        entity.Role = 1; // 1 = Admin
        //        var affected = await _userRepo.CreateAsync(entity);
        //        if (affected > 0)
        //        {
        //            var response = _mapper.Map<UserAdminResponse>(entity);
        //            return new ApiResponse<UserAdminResponse>
        //            {
        //                Success = true,
        //                Message = "Create successfully",
        //                Data = response
        //            };
        //        }
        //        return new ApiResponse<UserAdminResponse>
        //        {
        //            Success = false,
        //            Message = "Create failed",
        //            Data = null
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ApiResponse<UserAdminResponse>
        //        {
        //            Success = false,
        //            Message = $"Error: {ex.Message}",
        //            Data = null
        //        };
        //    }
        //}

        public async Task<ApiResponse<bool>> DeleteAsync(long id)
        {
            try
            {
                var existing = await _userRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "User not found",
                        Data = false
                    };

                var affected = await _userRepo.RemoveAsync(existing);
                return new ApiResponse<bool>
                {
                    Success = affected,
                    Message = affected ? "Deleted successfully" : "Delete failed",
                    Data = affected
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
        public async Task<User> GetUserbyUserId(long userId)
        {
            return await _userRepo.GetByIdAsync(userId);
        }
        public async Task<PagedResponse<UserResponse>> GetFilteredUsersAsync(UserGetRequest Filter, int page, int pageSize)
        {
            var filter = _mapper.Map<User>(Filter);
            var query = _userRepo.GetFiltered(filter);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResponse<UserResponse>
            {
                Items = _mapper.Map<IEnumerable<UserResponse>>(items),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<UserResponse>> UpdateAsync(long id, UserUpdateRequest request)
        {
            try
            {
                var existing = await _userRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<UserResponse>
                    {
                        Success = false,
                        Message = "User not found",
                        Data = null
                    };

                _mapper.Map(request, existing);

                if (request.AvatarFile != null)
                {
                    var imageUrl = await _photoService.UploadImageAsync(request.AvatarFile);
                    existing.Avatar = imageUrl;
                }

                if (!string.IsNullOrWhiteSpace(request.Password))
                {
                    existing.Password = HashPassword(request.Password);
                }

                var affected = await _userRepo.UpdateAsync(existing);
                if (affected > 0)
                {
                    var response = _mapper.Map<UserResponse>(existing);
                    return new ApiResponse<UserResponse>
                    {
                        Success = true,
                        Message = "Update successfully",
                        Data = response
                    };
                }

                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = "Update failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<User?> GetUserByUsername(string username)
        {
            return await _userRepo.GetUserByUsername(username);
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public Task<List<long>> GetUserFeaturesList(long userId)
        {
            return _userRepo.GetUserFeaturesList(userId);
        }

        private async Task CreateUserFeaturesForStaff(long staffUserId, long shopId, List<long>? enabledFeatureIds)
        {
            try
            {
                // 1. Lấy admin của shop để copy features
                var admin = await _userRepo.GetFirstUserAdmin(shopId);
                if (admin == null)
                {
                    // Nếu không có admin, không tạo features
                    return;
                }

                // 2. Lấy tất cả features của admin
                var adminFeatures = await _userFeatureRepo.GetByUserIdAsync(admin.UserId);
                if (!adminFeatures.Any())
                {
                    // Nếu admin không có features, không tạo gì
                    return;
                }

                // 3. Tạo user features cho staff (tất cả đều is_enabled = false)
                var staffFeatures = new List<UserFeature>();
                var enabledIds = enabledFeatureIds ?? new List<long>();

                foreach (var adminFeature in adminFeatures)
                {
                    var isEnabled = enabledIds.Contains(adminFeature.FeatureId);
                    
                    var staffFeature = new UserFeature
                    {
                        UserId = staffUserId,
                        FeatureId = adminFeature.FeatureId,
                        FeatureName = adminFeature.FeatureName,
                        IsEnabled = isEnabled,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    staffFeatures.Add(staffFeature);
                }

                // 4. Lưu vào database
                if (staffFeatures.Any())
                {
                    await _userFeatureRepo.AddRangeAsync(staffFeatures);
                    await _userFeatureRepo.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Log error nhưng không throw để không ảnh hưởng đến việc tạo user
                // Có thể log vào file hoặc database
                Console.WriteLine($"Error creating user features for staff {staffUserId}: {ex.Message}");
            }
        }
    }
}
