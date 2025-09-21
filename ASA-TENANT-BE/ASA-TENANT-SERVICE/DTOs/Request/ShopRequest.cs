using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Request
{
    public class ShopRequest
    {
        public string ShopName { get; set; }

        public string Address { get; set; }

        public string ShopToken { get; set; }

        public short? Status { get; set; }

        public string? QrcodeUrl { get; set; }

        public string? SepayApiKey { get; set; }

        public int? CurrentRequest { get; set; }

        public int? CurrentAccount { get; set; }

        public string? BankName { get; set; }

        public string? BankCode { get; set; }

        public string? BankNum { get; set; }
    }

    public class ShopGetRequest
    {
        public long? ShopId { get; set; }
        public string? ShopName { get; set; }  
        public string? Address { get; set; }
        public short? Status { get; set; }
        public string? QrcodeUrl { get; set; }
    }
}
