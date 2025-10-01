using System;
using System.ComponentModel.DataAnnotations;

namespace ASA_TENANT_SERVICE.DTOs.Request
{
    public class ShopSubscriptionRequest
    {
        [Required]
        public long shopId { get; set; }
        public long? platformProductId { get; set; }
        [Required]
        public DateTime startDate { get; set; }
        [Required]
        public DateTime endDate { get; set; }
        public short? status { get; set; }
    }

    public class ShopSubscriptionGetRequest
    {
        public long? shopSubscriptionId { get; set; }
        public long? shopId { get; set; }
        public long? platformProductId { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
        public short? status { get; set; }
    }
}


