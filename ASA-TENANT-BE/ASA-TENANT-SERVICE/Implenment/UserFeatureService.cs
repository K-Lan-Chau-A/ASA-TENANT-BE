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
    public class UserFeatureService : IUserFeatureService
    {
        private readonly UserFeatureRepo _userFeatureRepo;
        private readonly UserRepo _userRepo;
        private readonly IMapper _mapper;
        public UserFeatureService(UserFeatureRepo userFeatureRepo, IMapper mapper, UserRepo userRepo)
        {
            _userFeatureRepo = userFeatureRepo;
            _mapper = mapper;
            _userRepo = userRepo;
        }

        public async Task<ApiResponse<List<UserFeatureResponse>>> CreateAsync(UserFeatureRequest request)
        {
            try
            {
                // 1. Check user tồn tại
                var user = await _userRepo.GetByIdAsync(request.UserId);
                if (user == null)
                {
                    return new ApiResponse<List<UserFeatureResponse>>
                    {
                        Success = false,
                        Message = "User not found",
                        Data = null
                    };
                }

                // 2. Lấy tất cả feature của user trong DB
                var existingFeatures = await _userFeatureRepo.GetByUserIdAsync(request.UserId);

                // Chuẩn bị dictionary để check nhanh
                var existingDict = new Dictionary<long, UserFeature>();
                foreach (var ef in existingFeatures)
                {
                    if (!existingDict.ContainsKey(ef.FeatureId))
                    {
                        existingDict.Add(ef.FeatureId, ef);
                    }
                }

                var inputIds = new HashSet<long>();
                foreach (var f in request.Features)
                {
                    inputIds.Add(f.FeatureId);
                }

                var newFeatures = new List<UserFeature>();
                var removeFeatures = new List<UserFeature>();
                var keepFeatures = new List<UserFeature>();

                // 3. Xử lý giữ nguyên hoặc xoá
                foreach (var ef in existingFeatures)
                {
                    if (inputIds.Contains(ef.FeatureId))
                    {
                        keepFeatures.Add(ef); // giữ nguyên
                    }
                    else
                    {
                        removeFeatures.Add(ef); // xoá nếu không có trong input
                    }
                }

                if (removeFeatures.Count > 0)
                {
                    await _userFeatureRepo.RemoveRangeAsync(removeFeatures);
                }

                // 4. Xử lý thêm mới
                foreach (var f in request.Features)
                {
                    if (!existingDict.ContainsKey(f.FeatureId))
                    {
                        var nf = new UserFeature
                        {
                            UserId = request.UserId,
                            FeatureId = f.FeatureId,
                            FeatureName = f.FeatureName,
                            IsEnabled = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        newFeatures.Add(nf);
                    }
                }

                if (newFeatures.Count > 0)
                {
                    await _userFeatureRepo.AddRangeAsync(newFeatures);
                }

                await _userFeatureRepo.SaveChangesAsync();

                var allActive = new List<UserFeature>();
                allActive.AddRange(keepFeatures);
                allActive.AddRange(newFeatures);

                var response = new List<UserFeatureResponse>();
                foreach (var uf in allActive)
                {
                    response.Add(_mapper.Map<UserFeatureResponse>(uf));
                }

                return new ApiResponse<List<UserFeatureResponse>>
                {
                    Success = true,
                    Message = "Features updated successfully",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<UserFeatureResponse>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(long id)
        {
            try
            {
                var existing = await _userFeatureRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "UserFeature not found",
                        Data = false
                    };

                var affected = await _userFeatureRepo.RemoveAsync(existing);
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

        public async Task<PagedResponse<UserFeatureResponse>> GetFilteredUsersFeatureAsync(UserFeatureGetRequest Filter, int page, int pageSize)
        {
            var filter = _mapper.Map<UserFeature>(Filter);
            var query = _userFeatureRepo.GetFiltered(filter);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResponse<UserFeatureResponse>
            {
                Items = _mapper.Map<IEnumerable<UserFeatureResponse>>(items),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<List<UserFeatureResponse>>> UpdateAsync(UserFeatureUpdateRequest request)
        {
            try
            {
                var user = await _userRepo.GetByIdAsync(request.UserId);
                if (user == null)
                {
                    return new ApiResponse<List<UserFeatureResponse>>
                    {
                        Success = false,
                        Message = "User not found",
                        Data = null
                    };
                }

                // 2. Lấy danh sách feature hiện có trong DB
                var existingFeatures = await _userFeatureRepo.GetByUserIdAsync(request.UserId);

                var existingDict = new Dictionary<long, UserFeature>();
                foreach (var ef in existingFeatures)
                {
                    if (!existingDict.ContainsKey(ef.FeatureId))
                    {
                        existingDict.Add(ef.FeatureId, ef);
                    }
                }

                var notFoundFeatures = new List<long>();

                // 3. Update trạng thái theo request
                foreach (var f in request.Features)
                {
                    if (existingDict.ContainsKey(f.FeatureId))
                    {
                        var ef = existingDict[f.FeatureId];
                        ef.IsEnabled = f.IsEnable;
                        ef.UpdatedAt = DateTime.UtcNow;

                        await _userFeatureRepo.UpdateAsync(ef);
                    }
                    else
                    {
                        notFoundFeatures.Add(f.FeatureId);
                    }
                }

                // Nếu có feature không tồn tại thì báo lỗi
                if (notFoundFeatures.Count > 0)
                {
                    return new ApiResponse<List<UserFeatureResponse>>
                    {
                        Success = false,
                        Message = $"User does not have permission for FeatureIds: {string.Join(",", notFoundFeatures)}",
                        Data = null
                    };
                }

                // 4. SaveChanges
                await _userFeatureRepo.SaveChangesAsync();

                // 5. Build response
                var response = new List<UserFeatureResponse>();
                foreach (var ef in existingFeatures)
                {
                    response.Add(_mapper.Map<UserFeatureResponse>(ef));
                }

                return new ApiResponse<List<UserFeatureResponse>>
                {
                    Success = true,
                    Message = "Features updated successfully",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<UserFeatureResponse>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }
    }
}
