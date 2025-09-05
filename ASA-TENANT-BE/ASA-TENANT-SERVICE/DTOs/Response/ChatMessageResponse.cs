using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Response
{
    public class ChatMessageResponse
    {
        public long ChatMessageId { get; set; }

        public long? ShopId { get; set; }

        public long? UserId { get; set; }

        public string Content { get; set; }

        public string Sender { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
