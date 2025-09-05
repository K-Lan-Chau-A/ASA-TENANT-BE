using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Request
{
    public class NfcRequest
    {
        public short? Status { get; set; }
        public decimal? Balance { get; set; }
        public long? CustomerId { get; set; }
        [RegularExpression(@"^NFC.*$", ErrorMessage = "NFC Code must start with 'NFC'")]
        public string NfcCode { get; set; }
    }


    public class NfcGetRequest
    {
        public long? NfcId { get; set; }
        public short? Status { get; set; }
        public decimal? Balance { get; set; }
        public long? CustomerId { get; set; }
        [RegularExpression(@"^NFC.*$", ErrorMessage = "NFC Code must start with 'NFC'")]
        public string? NfcCode { get; set; }
    }
}
