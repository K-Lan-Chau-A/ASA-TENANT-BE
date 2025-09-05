using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Request
{
    public class FcmRequest
    {
        public string FcmToken { get; set; }

        public long? UserId { get; set; }
    }
    public class FcmGetRequest
    {
        public long? FcmId { get; set; }
        public string? FcmToken { get; set; }
        public long? UserId { get; set; }
    }
}
