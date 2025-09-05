using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Response
{
    public class OrderDetailResponse
    {
        public long OrderDetailId { get; set; }
        public int? Quantity { get; set; }
        public long? ProductUnitId { get; set; }
        public long? ProductId { get; set; }
        public decimal? TotalPrice { get; set; }
        public long? OrderId { get; set; }
    }
}


