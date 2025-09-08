using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Request
{
    public class PromptRequest
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Description { get; set; }
    }

    public class PromptGetRequest
    {
        public long PromptId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Description { get; set; }
    }
}
