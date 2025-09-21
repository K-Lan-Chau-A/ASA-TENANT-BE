using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Response
{
    public class ShopResponse
    {
        public long ShopId { get; set; }

        public string ShopName { get; set; }

        public string Address { get; set; }

        public string ShopToken { get; set; }

        public DateTime? CreatedAt { get; set; }

        public short? Status { get; set; }

        public string QrcodeUrl { get; set; }

        public string SepayApiKey { get; set; }

        public int? CurrentRequest { get; set; }

        public int? CurrentAccount { get; set; }

        public string BankName { get; set; }

        public string BankCode { get; set; }

        public string BankNum { get; set; }
    }
}
