using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Request
{
    public class FcmRequest
    {
        public int UserId { get; set; }
        public string FcmToken { get; set; }
        public string UniqueId { get; set; }
    }
    public class FcmGetRequest
    {
        public long? FcmId { get; set; }
        public int? UserId { get; set; }
        public string? FcmToken { get; set; }
        public string? UniqueId { get; set; }
        public bool? Isactive { get; set; }
    }
    public class FcmRefreshTokenRequest
    {
        public int UserId { get; set; }
        public string UniqueId { get; set; }   // hoặc DeviceId
        public string NewToken { get; set; }
    }
}
