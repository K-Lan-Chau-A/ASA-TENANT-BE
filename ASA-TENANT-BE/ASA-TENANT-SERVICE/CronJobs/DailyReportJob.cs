using Quartz;
using System;
using System.Threading.Tasks;
using ASA_TENANT_SERVICE.Interface;

namespace ASA_TENANT_SERVICE.CronJobs
{
    public class DailyReportJob : IJob
    {
        private readonly IReportService _reportService;

        public DailyReportJob(IReportService reportService)
        {
            _reportService = reportService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("🔔 DailyReportJob triggered at " + DateTime.Now);

            await _reportService.GenerateDailyReportAsync();
        }
    }
}
