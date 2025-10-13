using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Response
{
    public class ChatMessageWithAiResponse
    {
        public ChatMessageResponse UserMessage { get; set; }
        public ChatMessageResponse AiMessage { get; set; }
        public bool HasAiResponse { get; set; }
        public string Status { get; set; }
    }
}
