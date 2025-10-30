using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Response
{
    public class OrderResponse
    {
        public long OrderId { get; set; }
        public DateTime? Datetime { get; set; }
        public long? CustomerId { get; set; }
        public decimal? GrossRevenue { get; set; }
        public decimal? TotalPrice { get; set; }
        public decimal? TotalDiscount { get; set; }
        public decimal? FinalPrice { get; set; }
        public string PaymentMethod { get; set; }
        public short? Status { get; set; }
        public long? ShiftId { get; set; }
        public long? ShopId { get; set; }
        public long? VoucherId { get; set; }
        public decimal? Discount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string Note { get; set; }
        public List<OrderDetailResponse>? OrderDetails { get; set; }
    }
}


