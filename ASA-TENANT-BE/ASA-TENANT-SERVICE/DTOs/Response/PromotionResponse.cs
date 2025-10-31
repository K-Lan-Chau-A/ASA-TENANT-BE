using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Response
{
    public class PromotionResponse
    {
        public long PromotionId { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public decimal? Value { get; set; }
        public short? Type { get; set; }
        public short? Status { get; set; }
        public string Name { get; set; }
        public long? ShopId { get; set; }

        public IEnumerable<PromotionAppliedProductResponse>? AppliedProducts { get; set; }
    }
}
