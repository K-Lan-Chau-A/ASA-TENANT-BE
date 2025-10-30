using ASA_TENANT_REPO.DBContext;
using ASA_TENANT_REPO.Models;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.CronJobs
{
    public class PromotionExpiryJob : IJob
    {
        private readonly ASATENANTDBContext _context;

        public PromotionExpiryJob(ASATENANTDBContext context)
        {
            _context = context;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var nowDate = DateOnly.FromDateTime(DateTime.UtcNow);

                // 1) Deactivate Promotions whose EndDate < today
                var promotionsToDeactivate = await _context.Promotions
                    .Where(p => p.Status != 0 && p.EndDate != null && p.EndDate.Value < nowDate)
                    .ToListAsync();

                foreach (var promo in promotionsToDeactivate)
                {
                    promo.Status = 0;
                }

                // 2) Deactivate Promotions ending today but EndTime has passed (optional safety)
                var currentTime = TimeOnly.FromDateTime(DateTime.UtcNow);
                var promotionsEndTodayToDeactivate = await _context.Promotions
                    .Where(p => p.Status != 0
                                && p.EndDate != null && p.EndDate.Value == nowDate
                                && p.EndTime != null && currentTime > p.EndTime.Value)
                    .ToListAsync();

                foreach (var promo in promotionsEndTodayToDeactivate)
                {
                    promo.Status = 0;
                }


                var affected = await _context.SaveChangesAsync();
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] PromotionVoucherExpiryJob: Deactivated {affected} records");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] PromotionVoucherExpiryJob: Error {ex.Message}");
            }
        }
    }
}


