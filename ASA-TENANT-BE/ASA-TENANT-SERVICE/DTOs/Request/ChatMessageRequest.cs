using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Request
{
    public class ChatMessageRequest
    {
        public long? UserId { get; set; }

        public string Content { get; set; }

        public string Sender { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
    public class ChatMessageGetRequest
    {
        public long? ChatMessageId { get; set; }

        [Required(ErrorMessage = "ShopId is required")]
        public long ShopId { get; set; }

        public long? UserId { get; set; }

        public string? Content { get; set; }

        public string? Sender { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
