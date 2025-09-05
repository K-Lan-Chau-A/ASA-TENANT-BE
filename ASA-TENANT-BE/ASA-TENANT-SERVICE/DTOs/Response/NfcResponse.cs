using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Response
{
    public class NfcResponse
    {
        public long NfcId { get; set; }
        public short? Status { get; set; }
        public decimal? Balance { get; set; }
        public long? CustomerId { get; set; }
        public string NfcCode { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastUsedDate { get; set; }
        public string? CustomerFullName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerRank { get; set; }

    }
}
