using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Request
{
    public class UserRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public long? ShopId { get; set; }
        public short? Status { get; set; }
        public short? Role { get; set; }
        public string? Avatar { get; set; }
        public int? RequestLimit { get; set; }
        public int? AccountLimit { get; set; }
    }

    public class UserGetRequest
    {
        public long? UserId { get; set; }
        public string? Username { get; set; }
        public long? ShopId { get; set; }
        public short? Status { get; set; }
        public short? Role { get; set; }
        public int? RequestLimit { get; set; }
        public int? AccountLimit { get; set; }
    }
}
