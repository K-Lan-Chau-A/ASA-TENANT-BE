using ASA_TENANT_REPO.DBContext;
using ASA_TENANT_REPO.Models;
using ASA_TENANT_SERVICE.Interface;
using ASA_TENANT_SERVICE.Enums;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.CronJobs
{
    public class SubscriptionExpiryReminderJob : IJob
    {
        private readonly ASATENANTDBContext _dbContext;
        private readonly IFcmService _fcmService;
        private readonly IRealtimeNotifier _realtimeNotifier;

        public SubscriptionExpiryReminderJob(
            ASATENANTDBContext dbContext,
            IFcmService fcmService,
            IRealtimeNotifier realtimeNotifier)
        {
            _dbContext = dbContext;
            _fcmService = fcmService;
            _realtimeNotifier = realtimeNotifier;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var nowLocal = TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz);
                var todayLocal = nowLocal.Date; // Local date (SE Asia)

                // Build UTC range [startUtc, endUtc) for today .. today+7 inclusive
                var startOfTodayLocal = todayLocal; // 00:00 local
                var endOfWindowLocalExclusive = todayLocal.AddDays(8); // exclusive upper bound (after 7 days)
                var startUtc = TimeZoneInfo.ConvertTimeToUtc(startOfTodayLocal, tz);
                var endUtc = TimeZoneInfo.ConvertTimeToUtc(endOfWindowLocalExclusive, tz);

                var subs = await _dbContext.ShopSubscriptions
                    .Where(s => s.Status == 1 && s.EndDate >= startUtc && s.EndDate < endUtc)
                    .Select(s => new { s.ShopId, s.EndDate })
                    .ToListAsync();

                if (subs.Count == 0)
                    return;

                foreach (var sub in subs)
                {
                    if (!sub.ShopId.HasValue)
                        continue;

                    var shopId = sub.ShopId.Value;

                    // Convert end date to local for countdown
                    var subEndUtc = sub.EndDate.Kind == DateTimeKind.Unspecified
                        ? DateTime.SpecifyKind(sub.EndDate, DateTimeKind.Utc)
                        : sub.EndDate;
                    var endLocal = TimeZoneInfo.ConvertTimeFromUtc(subEndUtc, tz);
                    var daysLeft = (endLocal.Date - todayLocal).Days;
                    var title = "Gói dịch vụ sắp hết hạn";
                    var body = daysLeft == 0
                        ? $"Hôm nay là ngày hết hạn gói dịch vụ (ngày {endLocal:dd/MM/yyyy})."
                        : $"Gói dịch vụ sẽ hết hạn sau {daysLeft} ngày (ngày {endLocal:dd/MM/yyyy}).";

                    // Lấy đúng 1 admin của shop
                    var admin = await _dbContext.Users
                        .Where(u => u.ShopId == shopId && u.Role == 1 && u.Status == 1)
                        .Select(u => u.UserId)
                        .FirstOrDefaultAsync();

                    if (admin > 0)
                    {
                        // Lưu Notification theo admin
                        var startOfTodayUtcForGuardPerUser = startUtc;
                        var typeShortPerUser = (short)NotificationType.Warning;
                        var existedPerUser = await _dbContext.Notifications.AnyAsync(n =>
                            n.ShopId == shopId &&
                            n.UserId == admin &&
                            n.Type == typeShortPerUser &&
                            n.CreatedAt >= startOfTodayUtcForGuardPerUser &&
                            n.Title == title &&
                            n.Content == body);

                        if (!existedPerUser)
                        {
                            var entityUser = new Notification
                            {
                                ShopId = shopId,
                                UserId = admin,
                                Title = title,
                                Content = body,
                                Type = typeShortPerUser,
                                IsRead = false,
                                CreatedAt = DateTime.UtcNow
                            };
                            await _dbContext.Notifications.AddAsync(entityUser);
                            await _dbContext.SaveChangesAsync();
                        }

                        // Emit trực tiếp tới user
                        await _realtimeNotifier.EmitSubscriptionExpiryReminderToUser((long)admin, new
                        {
                            title,
                            body,
                            daysLeft,
                            endDate = endLocal
                        });

                        // Gửi FCM cho admin
                        await _fcmService.SendNotificationToManyUsersAsync(new List<long> { (long)admin }, title, body);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SubscriptionExpiryReminderJob error: {ex.Message}");
            }
        }
    }
}


