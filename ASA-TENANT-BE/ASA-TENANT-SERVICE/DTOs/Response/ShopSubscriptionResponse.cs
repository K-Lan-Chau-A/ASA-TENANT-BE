using System;

namespace ASA_TENANT_SERVICE.DTOs.Response
{
    public class ShopSubscriptionResponse
    {
        public long shopSubscriptionId { get; set; }
        public long? shopId { get; set; }
        public long? platformProductId { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public short? status { get; set; }
        public DateTime? createdAt { get; set; }
    }
}


