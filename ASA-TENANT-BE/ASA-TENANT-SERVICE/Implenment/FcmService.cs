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
    public class FcmService : IFcmService
    {
        private readonly FcmRepo _fcmRepo;
        private readonly IMapper _mapper;
        public FcmService(FcmRepo fcmRepo, IMapper mapper)
        {
            _fcmRepo = fcmRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse<FcmResponse>> CreateOrActiveAsync(FcmRequest request)
        {
            try
            {
                // tìm token theo UserId + UniqueId
                var existing = await _fcmRepo.GetFcmByUserIdAndUniqueIdAsync(request.UserId, request.UniqueId);

                if (existing != null)
                {
                    // nếu token chưa active hoặc token thay đổi thì update lại
                    if (existing.Isactive !=true || existing.FcmToken != request.FcmToken)
                    {
                        existing.Isactive = true;
                        existing.FcmToken = request.FcmToken;
                        existing.Updatedat = DateTime.Now;
                        existing.Lastlogin = DateTime.Now;

                        _fcmRepo.Update(existing);

                        var response = _mapper.Map<FcmResponse>(existing);
                        return new ApiResponse<FcmResponse>
                        {
                            Success = true,
                            Message = "Token updated successfully",
                            Data = response
                        };
                    }
                    else
                    {
                        // đã có token và đang active → không cần làm gì
                        var response = _mapper.Map<FcmResponse>(existing);
                        return new ApiResponse<FcmResponse>
                        {
                            Success = true,
                            Message = "Token already active",
                            Data = response
                        };
                    }
                }
                else
                {
                    // tạo mới
                    var entity = _mapper.Map<Fcm>(request);
                    entity.Isactive = true;
                    entity.Createdat = DateTime.Now;
                    entity.Updatedat = DateTime.Now;
                    entity.Lastlogin = DateTime.Now;

                    var affected = await _fcmRepo.CreateAsync(entity);

                    if (affected > 0)
                    {
                        var response = _mapper.Map<FcmResponse>(entity);
                        return new ApiResponse<FcmResponse>
                        {
                            Success = true,
                            Message = "Token created successfully",
                            Data = response
                        };
                    }

                    return new ApiResponse<FcmResponse>
                    {
                        Success = false,
                        Message = "Create failed",
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<FcmResponse>
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
                var existing = await _fcmRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Fcm not found",
                        Data = false
                    };

                var affected = await _fcmRepo.RemoveAsync(existing);
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

        public async Task<PagedResponse<FcmResponse>> GetFilteredFcmAsync(FcmGetRequest requestDto, int page, int pageSize)
        {
            var filter = _mapper.Map<Fcm>(requestDto);
            var query = _fcmRepo.GetFiltered(filter);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResponse<FcmResponse>
            {
                Items = _mapper.Map<IEnumerable<FcmResponse>>(items),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task LogoutDeviceAsync(FcmRequest request)
        {
            var existingToken = await _fcmRepo.GetFcmByUserIdAndUniqueIdAsync(request.UserId, request.UniqueId);
            if (existingToken != null && existingToken.Isactive == true)
            {
                existingToken.Isactive = false;
                existingToken.Updatedat = DateTime.Now;
                await _fcmRepo.UpdateAsync(existingToken);
            }
        }

        public async Task<bool> RefreshDeviceTokenAsync(FcmRefreshTokenRequest request)
        {
            var tokenRecord = await _fcmRepo.GetFcmByUserIdAndUniqueIdAsync(request.UserId, request.UniqueId);
            if (tokenRecord == null)
                return false;

            tokenRecord.FcmToken = request.NewToken;
            tokenRecord.Updatedat = DateTime.Now;
            tokenRecord.Lastlogin = DateTime.Now;
            await _fcmRepo.UpdateAsync(tokenRecord);
            return true;
        }

        public async Task<bool> SendNotificationToManyUsersAsync(List<long> userIds, string title, string body)
        {
            Console.WriteLine($"SendNotificationToManyUsersAsync: userIds = [{string.Join(", ", userIds)}], title = '{title}', body = '{body}'");
            
            var listTokens = new List<string>();
            bool allSuccess = false;
            foreach (var userId in userIds)
            {
                var tokens = await _fcmRepo.GetActiveTokensByUserIdAsync(userId);
                Console.WriteLine($"User {userId}: found {tokens?.Count ?? 0} active FCM tokens");
                
                if (tokens == null || tokens.Count == 0)
                {
                    Console.WriteLine($"User {userId}: no active FCM tokens");
                    allSuccess = false;
                    continue;
                }
                foreach (var t in tokens)
                {
                    if (!string.IsNullOrEmpty(t.FcmToken))
                    {
                        listTokens.Add(t.FcmToken);
                    }
                }
            }
            
            Console.WriteLine($"Total FCM tokens collected: {listTokens.Count}");
            
            if (listTokens.Count > 0)
            {
                allSuccess = await SendNotificationAsync(listTokens, title, body);
            }
            else
            {
                Console.WriteLine("No valid FCM tokens found for any users");
            }
            return allSuccess;
        }
        private async Task<bool> SendNotificationAsync(List<string> tokens, string title, string body)
        {
            Console.WriteLine($"SendNotificationAsync: tokens count = {tokens?.Count ?? -1}, title = '{title}', body = '{body}'");
            
            if (tokens == null || tokens.Count == 0)
            {
                Console.WriteLine("No FCM tokens provided, skipping notification");
                return false;
            }

            var message = new FirebaseAdmin.Messaging.MulticastMessage
            {
                Tokens = tokens,
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = title,
                    Body = body
                }
            };

            var messaging = FirebaseAdmin.Messaging.FirebaseMessaging.DefaultInstance;
            if (messaging == null)
            {
                Console.WriteLine("FirebaseMessaging.DefaultInstance is null!");
                return false;
            }

            try
            {
                var response = await messaging.SendEachForMulticastAsync(message);
                Console.WriteLine($"FCM sent: SuccessCount = {response.SuccessCount}, FailureCount = {response.FailureCount}");
                return response.SuccessCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("SendMulticastAsync error: " + ex);
                throw;
            }
        }

        public async Task<ApiResponse<FcmResponse>> UpdateAsync(long id, FcmRequest request)
        {
            try
            {
                var existing = await _fcmRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<FcmResponse>
                    {
                        Success = false,
                        Message = "Fcm not found",
                        Data = null
                    };

                // Map dữ liệu từ DTO sang entity, bỏ Id
                _mapper.Map(request, existing);

                var affected = await _fcmRepo.UpdateAsync(existing);
                if (affected > 0)
                {
                    var response = _mapper.Map<FcmResponse>(existing);
                    return new ApiResponse<FcmResponse>
                    {
                        Success = true,
                        Message = "Update successfully",
                        Data = response
                    };
                }

                return new ApiResponse<FcmResponse>
                {
                    Success = false,
                    Message = "Update failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<FcmResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }
    }
}
