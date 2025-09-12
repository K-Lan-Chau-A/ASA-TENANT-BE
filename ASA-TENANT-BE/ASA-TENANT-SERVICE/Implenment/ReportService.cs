using ASA_TENANT_REPO.DBContext;
using ASA_TENANT_REPO.Models;
using ASA_TENANT_REPO.Repository;
using ASA_TENANT_SERVICE.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Implenment
{
    public class ReportService : IReportService
    {
        private readonly ReportRepo _reportRepo;
        public ReportService(ReportRepo reportRepo)
        {
            _reportRepo = reportRepo;
        }

        public async Task GenerateDailyReportAsync()
        {
            await _reportRepo.GenerateDailyReportAsync();

        }

        public async Task GenerateMonthlyReportAsync()
        {
            await _reportRepo.GenerateMonthlyReportAsync();
        }
    }
}
