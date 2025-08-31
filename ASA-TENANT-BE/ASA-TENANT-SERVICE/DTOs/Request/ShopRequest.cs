using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Request
{
    public class ShopRequest
    {
        public string ShopName { get; set; }
        public string Subscription { get; set; }
        public string Address { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public short? Status { get; set; }
        public string QrcodeUrl { get; set; }
    }

    public class ShopGetRequest
    {
        public long? ShopId { get; set; }
        public string? ShopName { get; set; }
        public string? Subscription { get; set; }
        public string? Address { get; set; }
        public short? Status { get; set; }
        public string? QrcodeUrl { get; set; }
    }
}
