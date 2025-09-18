using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Request
{
    public class NotificationRequest
    {
        public long? ShopId { get; set; }

        public long? UserId { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public short? Type { get; set; }

        public bool? IsRead { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
    public class NotificationGetRequest
    {
        public long? NotificationId { get; set; }

        [Required(ErrorMessage = "ShopId is required")]
        public long ShopId { get; set; }

        public long? UserId { get; set; }

        public string? Title { get; set; }

        public string? Content { get; set; }

        public short? Type { get; set; }

        public bool? IsRead { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
