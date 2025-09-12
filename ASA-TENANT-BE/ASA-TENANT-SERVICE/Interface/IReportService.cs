using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Interface
{
    public interface IReportService
    {
        Task GenerateDailyReportAsync();
        Task GenerateMonthlyReportAsync();
    }
}
