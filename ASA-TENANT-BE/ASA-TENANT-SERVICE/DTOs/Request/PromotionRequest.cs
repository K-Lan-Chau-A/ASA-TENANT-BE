using ASA_TENANT_SERVICE.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Request
{
    public class PromotionRequest
    {
        public long ShopId { get; set; }
        public string? Name { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public decimal? Value { get; set; }
        [EnumDataType(typeof(PromotionType))]
        public PromotionType Type { get; set; }
        public short? Status { get; set; }
        public HashSet<long>? ProductIds { get; set; }
    }

    public class PromotionGetRequest
    {
        public long PromotionId { get; set; }
        public long ShopId { get; set; }
        public string? Name { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public decimal? Value { get; set; }
        public short? Type { get; set; }
        public short? Status { get; set; }
    }
}

