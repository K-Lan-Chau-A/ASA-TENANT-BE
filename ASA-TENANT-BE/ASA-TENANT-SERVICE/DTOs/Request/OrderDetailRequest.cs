using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Request
{
    public class OrderDetailRequest
    {
        public int? Quantity { get; set; }
        public long? ProductUnitId { get; set; }
        public long? ProductId { get; set; }
    }

    public class OrderDetailGetRequest
    {
        public long? OrderDetailId { get; set; }
        public int? Quantity { get; set; }
        public long? ProductUnitId { get; set; }
        public long? ProductId { get; set; }
        public long? OrderId { get; set; }
        public decimal? BasePrice { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? FinalPrice { get; set; }
        public decimal? Profit { get; set; }
    }
}


