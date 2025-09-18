using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Request
{
    public class LogActivityRequest
    {
        public long UserId { get; set; }
        public string Content { get; set; }
        public int Type { get; set; }
        public long ShopId { get; set; }
    }

    public class LogActivityGetRequest
    {
        public long? LogActivityId { get; set; }
        public long? UserId { get; set; }
        public string? Content { get; set; }
        public int? Type { get; set; }

        [Required(ErrorMessage = "ShopId is required")]
        public long ShopId { get; set; }
    }
}
