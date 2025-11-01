using ASA_TENANT_REPO.DBContext;
using ASA_TENANT_REPO.Models;
using ASA_TENANT_REPO.Repository;
using ASA_TENANT_SERVICE.DTOs.Common;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Interface;
using ASA_TENANT_SERVICE.Enums;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Implenment
{
    public class CustomerService : ICustomerService
    {
        private readonly CustomerRepo _customerRepo;
        private readonly IMapper _mapper;
        private readonly ASATENANTDBContext _context;
        private readonly IFcmService _fcmService;
        private readonly IRealtimeNotifier _realtimeNotifier;
        
        public CustomerService(CustomerRepo customerRepo, IMapper mapper, ASATENANTDBContext context, IFcmService fcmService, IRealtimeNotifier realtimeNotifier)
        {
            _customerRepo = customerRepo;
            _mapper = mapper;
            _context = context;
            _fcmService = fcmService;
            _realtimeNotifier = realtimeNotifier;
        }

        public async Task<ApiResponse<CustomerResponse>> CreateAsync(CustomerRequest request)
        {
            try
            {
                var entity = _mapper.Map<Customer>(request);

                // Normalize inputs
                var normalizedEmail = (entity.Email ?? string.Empty).Trim().ToLowerInvariant();
                var normalizedPhone = (entity.Phone ?? string.Empty).Trim();

                // Duplicate checks per shop
                if (!string.IsNullOrWhiteSpace(normalizedEmail))
                {
                    var emailExists = await _context.Customers
                        .AnyAsync(c => c.ShopId == request.ShopId && c.Email != null && c.Email.ToLower() == normalizedEmail);
                    if (emailExists)
                    {
                        return new ApiResponse<CustomerResponse>
                        {
                            Success = false,
                            Message = "Email already exists",
                            Data = null
                        };
                    }
                }

                if (!string.IsNullOrWhiteSpace(normalizedPhone))
                {
                    var phoneExists = await _context.Customers
                        .AnyAsync(c => c.ShopId == request.ShopId && c.Phone != null && c.Phone == normalizedPhone);
                    if (phoneExists)
                    {
                        return new ApiResponse<CustomerResponse>
                        {
                            Success = false,
                            Message = "Phone already exists",
                            Data = null
                        };
                    }
                }

                // persist normalized values
                entity.Email = string.IsNullOrWhiteSpace(normalizedEmail) ? null : normalizedEmail;
                entity.Phone = string.IsNullOrWhiteSpace(normalizedPhone) ? null : normalizedPhone;

                // Auto-assign RankId: pick the rank with lowest Benefit for this shop; if no ranks, keep null
                if (request.ShopId > 0)
                {
                    var lowestBenefitRank = await _context.Ranks
                        .Where(r => r.ShopId == request.ShopId)
                        .OrderBy(r => r.Benefit)
                        .FirstOrDefaultAsync();

                    entity.RankId = lowestBenefitRank?.RankId;
                }
                else
                {
                    entity.RankId = null;
                }

                // Defaults: Spent = 0, Status = 1 when not provided by FE
                entity.Spent = entity.Spent ?? 0m;
                entity.Status = entity.Status ?? 1;

                var affected = await _customerRepo.CreateAsync(entity);

                if (affected > 0)
                {
                    var response = _mapper.Map<CustomerResponse>(entity);
                    return new ApiResponse<CustomerResponse>
                    {
                        Success = true,
                        Message = "Create successfully",
                        Data = response
                    };
                }

                return new ApiResponse<CustomerResponse>
                {
                    Success = false,
                    Message = "Create failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<CustomerResponse>
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
                var existing = await _customerRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Customer not found",
                        Data = false
                    };

                var affected = await _customerRepo.RemoveAsync(existing);
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

        public async Task<PagedResponse<CustomerResponse>> GetFilteredCustomersAsync(CustomerGetRequest Filter, int page, int pageSize)
        {
            var filter = _mapper.Map<Customer>(Filter);

            var query = _customerRepo.GetFiltered(filter);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResponse<CustomerResponse>
            {
                Items = _mapper.Map<IEnumerable<CustomerResponse>>(items),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

        }

        public async Task<ApiResponse<CustomerResponse>> UpdateAsync(long id, CustomerRequest request)
        {
            try
            {
                var existing = await _customerRepo.GetByIdAsync(id);
                if (existing == null)
                    return new ApiResponse<CustomerResponse>
                    {
                        Success = false,
                        Message = "Customer not found",
                        Data = null
                    };

                // Map dữ liệu từ DTO sang entity, bỏ Id
                _mapper.Map(request, existing);
                existing.UpdatedAt = DateTime.UtcNow;

                var affected = await _customerRepo.UpdateAsync(existing);
                if (affected > 0)
                {
                    var response = _mapper.Map<CustomerResponse>(existing);
                    return new ApiResponse<CustomerResponse>
                    {
                        Success = true,
                        Message = "Update successfully",
                        Data = response
                    };
                }

                return new ApiResponse<CustomerResponse>
                {
                    Success = false,
                    Message = "Update failed",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<CustomerResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<bool>> UpdateCustomerSpentAndRankAsync(long customerId, decimal orderTotalPrice)
        {
            try
            {
                Console.WriteLine($"=== UpdateCustomerSpentAndRankAsync: CustomerId={customerId}, OrderTotalPrice={orderTotalPrice}");
                
                var customer = await _context.Customers
                    .Include(c => c.Rank)
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId);

                if (customer == null)
                {
                    Console.WriteLine($"Customer {customerId} not found");
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Customer not found",
                        Data = false
                    };
                }

                var oldSpent = customer.Spent ?? 0;
                var oldRankId = customer.RankId;
                
                // Cộng TotalPrice vào Spent
                customer.Spent = oldSpent + orderTotalPrice;
                customer.UpdatedAt = DateTime.UtcNow;

                Console.WriteLine($"Customer {customerId}: Spent {oldSpent} + {orderTotalPrice} = {customer.Spent}, Current Rank = {oldRankId}");

                // Kiểm tra và cập nhật rank nếu cần
                await UpdateCustomerRankAsync(customer);

                var affected = await _context.SaveChangesAsync();
                
                return new ApiResponse<bool>
                {
                    Success = affected > 0,
                    Message = affected > 0 ? "Customer spent and rank updated successfully" : "Update failed",
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

        private async Task UpdateCustomerRankAsync(Customer customer)
        {
            try
            {
                // Lấy tất cả ranks của shop theo thứ tự threshold tăng dần
                var ranks = await _context.Ranks
                    .Where(r => r.ShopId == customer.ShopId)
                    .OrderBy(r => r.Threshold)
                    .ToListAsync();

                if (!ranks.Any()) return;

                // Logic: Nếu spent vượt qua threshold của rank hiện tại thì lên rank tiếp theo
                int? newRankId = customer.RankId; // Mặc định giữ nguyên rank hiện tại
                
                // Tìm rank hiện tại của customer
                var currentRank = ranks.FirstOrDefault(r => r.RankId == customer.RankId);
                if (currentRank == null) return; // Không tìm thấy rank hiện tại
                
                // Nếu rank hiện tại có threshold null (rank cao nhất), không thể lên cao hơn
                if (currentRank.Threshold == null)
                {
                    Console.WriteLine($"Customer {customer.CustomerId} đã ở rank cao nhất (Kim Cương)");
                    return;
                }
                
                // Kiểm tra xem spent có vượt qua threshold của rank hiện tại không
                if (customer.Spent >= (decimal)currentRank.Threshold)
                {
                    // Tìm rank tiếp theo (rank có threshold cao hơn)
                    var nextRank = ranks
                        .Where(r => r.Threshold != null && r.Threshold > currentRank.Threshold)
                        .OrderBy(r => r.Threshold)
                        .FirstOrDefault();
                    
                    if (nextRank != null)
                    {
                        newRankId = nextRank.RankId;
                        Console.WriteLine($"Customer {customer.CustomerId} vượt qua threshold {currentRank.Threshold} của rank {currentRank.RankName}, lên rank {nextRank.RankName} (threshold: {nextRank.Threshold})");
                    }
                    else
                    {
                        // Nếu không có rank tiếp theo, lên rank cao nhất (Kim Cương)
                        var highestRank = ranks.FirstOrDefault(r => r.Threshold == null);
                        if (highestRank != null)
                        {
                            newRankId = highestRank.RankId;
                            Console.WriteLine($"Customer {customer.CustomerId} vượt qua tất cả threshold, lên rank cao nhất {highestRank.RankName}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Customer {customer.CustomerId} spent {customer.Spent} chưa vượt qua threshold {currentRank.Threshold} của rank {currentRank.RankName}");
                }

                // Debug log để kiểm tra
                Console.WriteLine($"Customer {customer.CustomerId}: Spent = {customer.Spent}, Current Rank = {customer.RankId}, New Rank = {newRankId}");
                Console.WriteLine($"Available ranks for shop {customer.ShopId}:");
                foreach (var rank in ranks)
                {
                    Console.WriteLine($"  RankId: {rank.RankId}, Name: {rank.RankName}, Threshold: {rank.Threshold}");
                }

                // Cập nhật rank nếu có thay đổi
                if (newRankId.HasValue && customer.RankId != newRankId.Value)
                {
                    var oldRankId = customer.RankId;
                    customer.RankId = newRankId.Value;
                    
                    // Lấy thông tin rank cũ và mới
                    var oldRank = ranks.FirstOrDefault(r => r.RankId == oldRankId);
                    var newRank = ranks.FirstOrDefault(r => r.RankId == newRankId.Value);
                    
                    Console.WriteLine($"Updated customer {customer.CustomerId} rank from {oldRank?.RankName} to {newRank?.RankName}");
                    
                    // Gửi notification khi customer lên rank mới
                    if (newRank != null)
                    {
                        await SendCustomerRankUpNotificationAsync(customer, oldRank, newRank);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error nhưng không throw để không ảnh hưởng đến việc cập nhật spent
                Console.WriteLine($"Error updating customer rank: {ex.Message}");
            }
        }

        private async Task SendCustomerRankUpNotificationAsync(Customer customer, Rank oldRank, Rank newRank)
        {
            try
            {
                Console.WriteLine($"=== SendCustomerRankUpNotificationAsync: Customer {customer.CustomerId} ({customer.FullName}) từ rank {oldRank?.RankName ?? "Chưa có"} lên {newRank.RankName}");
                
                // Kiểm tra xem đã gửi notification cho customer này trong 10 phút gần đây chưa
                var recentNotification = await _context.Notifications
                    .Where(n => n.ShopId == customer.ShopId 
                        && n.Title.Contains("Khách hàng lên hạng thành viên")
                        && n.Content.Contains(customer.FullName)
                        && n.CreatedAt >= DateTime.UtcNow.AddMinutes(-10))
                    .FirstOrDefaultAsync();

                if (recentNotification != null)
                {
                    Console.WriteLine($"⚠️ Đã gửi notification rank up cho customer {customer.CustomerId} trong 10 phút gần đây, bỏ qua để tránh spam");
                    return;
                }
                
                var title = "Khách hàng lên hạng thành viên!";
                var body = $"Khách hàng {customer.FullName} đã lên từ hạng {oldRank?.RankName ?? "Chưa có"} lên hạng {newRank.RankName}!";

                // Lấy tất cả user trong shop
                var shopUsers = await _context.Users
                    .Where(u => u.ShopId == customer.ShopId && u.Status == 1)
                    .Select(u => u.UserId)
                    .ToListAsync();

                Console.WriteLine($"Shop {customer.ShopId} có {shopUsers.Count} user active: [{string.Join(", ", shopUsers)}]");

                if (shopUsers.Any())
                {
                    // Gửi FCM notification (không throw exception nếu fail)
                    try
                    {
                        var fcmSuccess = await _fcmService.SendNotificationToManyUsersAsync(shopUsers, title, body);
                        Console.WriteLine($"FCM notification result: {fcmSuccess}");
                    }
                    catch (Exception fcmEx)
                    {
                        Console.WriteLine($"FCM notification failed: {fcmEx.Message}");
                    }

                    // Gửi SignalR notification tới shop group
                    try
                    {
                        await _realtimeNotifier.EmitCustomerRankUpToShop(customer.ShopId ?? 0, new
                        {
                            title,
                            body,
                            type = 0, 
                            customerId = customer.CustomerId,
                            customerName = customer.FullName,
                            oldRankName = oldRank?.RankName ?? "Chưa có",
                            newRankName = newRank.RankName,
                            shopId = customer.ShopId
                        });
                        Console.WriteLine($"SignalR notification sent to Shop_{customer.ShopId}");
                    }
                    catch (Exception signalREx)
                    {
                        Console.WriteLine($"SignalR notification failed: {signalREx.Message}");
                    }

                    // Lưu notification vào database cho tất cả user trong shop
                    try
                    {
                        var notifications = shopUsers.Select(userId => new Notification
                        {
                            ShopId = customer.ShopId,
                            UserId = userId,
                            Title = title,
                            Content = body,
                            Type = 0, 
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow
                        }).ToList();

                        await _context.Notifications.AddRangeAsync(notifications);
                        await _context.SaveChangesAsync();
                        Console.WriteLine($"Database notifications saved: {notifications.Count} records");
                    }
                    catch (Exception dbEx)
                    {
                        Console.WriteLine($"Database notification failed: {dbEx.Message}");
                    }

                    Console.WriteLine($"✅ Đã gửi notification rank up cho {shopUsers.Count} user trong shop {customer.ShopId}");
                }
                else
                {
                    Console.WriteLine($"⚠️ Shop {customer.ShopId} không có user active nào");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error sending customer rank up notification: {ex.Message}");
                Console.WriteLine($"   Stack trace: {ex.StackTrace}");
            }
        }
    }
}
