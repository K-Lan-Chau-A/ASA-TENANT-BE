using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Response
{
    public class PromotionProductResponse
    {
        public long PromotionProductId { get; set; }
        public long? PromotionId { get; set; }
        public long? ProductId { get; set; }
        public long? UnitId { get; set; }
        public string? PromotionName { get; set; }
        public short? PromotionType { get; set; }
        public decimal? PromotionValue { get; set; }
        public DateOnly? PromotionStartDate { get; set; }
        public DateOnly? PromotionEndDate { get; set; }
        public TimeOnly? PromotionStartTime { get; set; }
        public TimeOnly? PromotionEndTime { get; set; }
        public string? ProductName { get; set; }
        public string? UnitName { get; set; }

    }
}
