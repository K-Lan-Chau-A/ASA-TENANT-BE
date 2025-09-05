using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Response
{
    public class TransactionResponse
    {
        public long TransactionId { get; set; }
        public long? OrderId { get; set; }
        public long? UserId { get; set; }
        public string PaymentStatus { get; set; }
        public string AppTransId { get; set; }
        public string ZpTransId { get; set; }
        public int? ReturnCode { get; set; }
        public string ReturnMessage { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}


