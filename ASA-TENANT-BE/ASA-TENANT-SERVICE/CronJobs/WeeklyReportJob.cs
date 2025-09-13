using Quartz;
using System;
using System.Threading.Tasks;
using ASA_TENANT_SERVICE.Interface;

namespace ASA_TENANT_SERVICE.CronJobs
{
    public class WeeklyReportJob : IJob
    {
        private readonly IReportService _reportService;

        public WeeklyReportJob(IReportService reportService)
        {
            _reportService = reportService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("🔔 WeeklyReportJob triggered at " + DateTime.Now);

            await _reportService.GenerateWeeklyReportAsync();
        }
    }
}
