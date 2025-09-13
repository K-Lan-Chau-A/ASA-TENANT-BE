using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Response
{
    public class ReportResponse
    {
        public long ReportId { get; set; }
        public short? Type { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public DateTime? CreateAt { get; set; }
        public decimal? Revenue { get; set; }
        public int? OrderCounter { get; set; }
        public long? ShopId { get; set; }
        public decimal? GrossProfit { get; set; }
        public decimal? Cost { get; set; }
        public List<ReportDetailResponse> ReportDetails { get; set; } = new List<ReportDetailResponse>();
    }
}
