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
    public class NotificationService : INotificationService
    {
        private readonly NotificationRepo _notificationRepo;
        private readonly IMapper _mapper;
        private readonly ASATENANTDBContext _dbContext;
        private readonly UserRepo _userRepo;
        private readonly IFcmService _fcmService;
        private readonly IRealtimeNotifier _realtimeNotifier;
        
        public NotificationService(NotificationRepo notificationRepo, IMapper mapper, 
            ASATENANTDBContext dbContext, UserRepo userRepo, 
            IFcmService fcmService, IRealtimeNotifier realtimeNotifier)
        {
            _notificationRepo = notificationRepo;
            _mapper = mapper;
            _dbContext = dbContext;
            _userRepo = userRepo;
            _fcmService = fcmService;
            _realtimeNotifier = realtimeNotifier;
        }

        public async Task<ApiResponse<NotificationResponse>> CreateAsync(NotificationRequest request)
        {
            try
            {
                var entity = _mapper.Map<Notification>(request);

                var affected = await _notificationRepo.CreateAsync(entity);

                if (affected > 0)
                {
                    var response = _mapper.Map<NotificationResponse>(entity);
                    return new ApiResponse<NotificationResponse>
                    {
                        Success = true,
                        Message = "Create successfully",
                        Data = response
                    };
                }

                return new ApiResponse<NotificationResponse>
                {
                    Success = false,
                    Message = "Create failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<NotificationResponse>
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
                var existing = await _notificationRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Notification not found",
                        Data = false
                    };

                var affected = await _notificationRepo.RemoveAsync(existing);
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

        public async Task<PagedResponse<NotificationResponse>> GetFilteredCategoriesAsync(NotificationGetRequest Filter, int page, int pageSize)
        {
            var filter = _mapper.Map<Notification>(Filter);
            var query = _notificationRepo.GetFiltered(filter);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResponse<NotificationResponse>
            {
                Items = _mapper.Map<IEnumerable<NotificationResponse>>(items),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<NotificationResponse>> UpdateAsync(long id, NotificationRequest request)
        {
            try
            {
                var existing = await _notificationRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<NotificationResponse>
                    {
                        Success = false,
                        Message = "Notification not found",
                        Data = null
                    };

                // Map dữ liệu từ DTO sang entity, bỏ Id
                _mapper.Map(request, existing);

                var affected = await _notificationRepo.UpdateAsync(existing);
                if (affected > 0)
                {
                    var response = _mapper.Map<NotificationResponse>(existing);
                    return new ApiResponse<NotificationResponse>
                    {
                        Success = true,
                        Message = "Update successfully",
                        Data = response
                    };
                }

                return new ApiResponse<NotificationResponse>
                {
                    Success = false,
                    Message = "Update failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<NotificationResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<bool>> MarkAsReadAsync(long id)
        {
            try
            {
                var existing = await _notificationRepo.GetByIdAsync(id);
                if (existing == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Notification not found",
                        Data = false
                    };
                }

                existing.IsRead = true;
                var affected = await _notificationRepo.UpdateAsync(existing);
                return new ApiResponse<bool>
                {
                    Success = affected > 0,
                    Message = affected > 0 ? "Đánh dấu đã đọc" : "Update failed",
                    Data = affected > 0
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

        public async Task<ApiResponse<int>> MarkAllAsReadByUserAsync(long userId)
        {
            try
            {
                var query = _notificationRepo.GetFiltered(new Notification { UserId = userId, IsRead = false });
                var list = await query.ToListAsync();
                foreach (var n in list)
                {
                    n.IsRead = true;
                }
                var count = 0;
                foreach (var n in list)
                {
                    count += await _notificationRepo.UpdateAsync(n) > 0 ? 1 : 0;
                }
                return new ApiResponse<int>
                {
                    Success = true,
                    Message = "Đánh dấu tất cả đã đọc",
                    Data = count
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<int>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = 0
                };
            }
        }

        public async Task<ApiResponse<bool>> BroadcastToAllShopsAsync(BroadcastNotificationRequest request)
        {
            try
            {
                Console.WriteLine($"BroadcastToAllShopsAsync: Starting broadcast to all shops");
                Console.WriteLine($"Title: {request.Title}, Content: {request.Content}, Type: {request.Type}");

                // Lấy tất cả user active từ tất cả shop
                var allUsers = await _userRepo.GetFiltered(new User { Status = 1 })
                    .Select(u => new { u.UserId, u.ShopId })
                    .ToListAsync();

                Console.WriteLine($"Found {allUsers.Count} active users across all shops");

                var totalNotifications = 0;
                var successfulNotifications = 0;

                // Group users by shop để gửi SignalR theo shop
                var usersByShop = allUsers.GroupBy(u => u.ShopId).ToList();

                foreach (var shopGroup in usersByShop)
                {
                    var shopId = shopGroup.Key ?? 0; // Handle null shopId
                    var userIds = shopGroup.Select(u => u.UserId).ToList();

                    Console.WriteLine($"Processing Shop {shopId} with {userIds.Count} users");

                    // Gửi SignalR cho shop này
                    try
                    {
                        await _realtimeNotifier.EmitLowStockAlertToShop(shopId, new
                        {
                            title = request.Title,
                            content = request.Content,
                            type = request.Type
                        });
                        Console.WriteLine($"SignalR sent to Shop_{shopId}");
                    }
                    catch (Exception signalREx)
                    {
                        Console.WriteLine($"SignalR failed for Shop_{shopId}: {signalREx.Message}");
                    }

                    // Gửi FCM cho tất cả user trong shop này
                    try
                    {
                        var fcmSuccess = await _fcmService.SendNotificationToManyUsersAsync(userIds, request.Title, request.Content);
                        Console.WriteLine($"FCM sent to {userIds.Count} users in Shop_{shopId}, Success: {fcmSuccess}");
                    }
                    catch (Exception fcmEx)
                    {
                        Console.WriteLine($"FCM failed for Shop_{shopId}: {fcmEx.Message}");
                    }

                    // Lưu notification vào DB cho từng user
                    foreach (var userId in userIds)
                    {
                        try
                        {
                            var notification = new Notification
                            {
                                UserId = userId,
                                ShopId = shopId,
                                Title = request.Title,
                                Content = request.Content,
                                Type = request.Type,
                                IsRead = false,
                                CreatedAt = DateTime.UtcNow
                            };

                            var affected = await _notificationRepo.CreateAsync(notification);
                            totalNotifications++;
                            if (affected > 0)
                            {
                                successfulNotifications++;
                            }
                        }
                        catch (Exception dbEx)
                        {
                            Console.WriteLine($"Failed to save notification for User {userId}: {dbEx.Message}");
                            totalNotifications++;
                        }
                    }
                }

                Console.WriteLine($"Broadcast completed: {successfulNotifications}/{totalNotifications} notifications saved");

                return new ApiResponse<bool>
                {
                    Success = successfulNotifications > 0,
                    Message = $"Broadcast completed. {successfulNotifications}/{totalNotifications} notifications sent successfully.",
                    Data = successfulNotifications > 0
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BroadcastToAllShopsAsync error: {ex.Message}");
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
