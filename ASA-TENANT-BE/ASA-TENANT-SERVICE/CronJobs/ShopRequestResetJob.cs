using ASA_TENANT_REPO.Repository;
using Quartz;
using System;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.CronJobs
{
    public class ShopRequestResetJob : IJob
    {
        private readonly ShopRepo _shopRepo;

        public ShopRequestResetJob(ShopRepo shopRepo)
        {
            _shopRepo = shopRepo;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine($"[CRON] ShopRequestResetJob triggered at {DateTime.Now}");
            var allShops = await _shopRepo.GetAllAsync();
            int count = 0;
            foreach (var shop in allShops)
            {
                if ((shop.CurrentRequest ?? 0) != 0) {
                    shop.CurrentRequest = 0;
                    await _shopRepo.UpdateAsync(shop);
                    count++;
                }
            }
            Console.WriteLine($"[CRON] Reset currentRequest thành công cho {count} shop!");
        }
    }
}
