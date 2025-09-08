using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Response
{
    public class NotificationResponse
    {
        public long NotificationId { get; set; }

        public long? ShopId { get; set; }

        public long? UserId { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public short? Type { get; set; }

        public bool? IsRead { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
