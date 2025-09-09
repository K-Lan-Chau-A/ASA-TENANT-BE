using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Response
{
    public class UserResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public short? Status { get; set; }
        public long? ShopId { get; set; }
        public short? Role { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string Avatar { get; set; }
    }
}
