using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Request
{
    public class VoucherRequest
    {
        public decimal? Value { get; set; }
        public DateTime? CreatedAt { get; set; }
        public short? Type { get; set; }
        public DateTime? Expired { get; set; }
        public long ShopId { get; set; }
        public string? Code { get; set; }
    }

    public class VoucherGetRequest
    {
        public long? VoucherId { get; set; }
        public decimal? Value { get; set; }
        public DateTime? CreatedAt { get; set; }
        public short? Type { get; set; }
        public DateTime? Expired { get; set; }
        public long? ShopId { get; set; }
        public string? Code { get; set; }
    }
}


