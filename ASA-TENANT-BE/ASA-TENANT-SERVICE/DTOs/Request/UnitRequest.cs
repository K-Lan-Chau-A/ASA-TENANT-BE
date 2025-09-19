using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Request
{
    public class UnitRequest
    {
        public long? ShopId { get; set; }
        public string? Name { get; set; }
    }

    public class UnitGetRequest
    {
        public long UnitId { get; set; }
        [Required(ErrorMessage = "ShopId is required")]
        public long ShopId { get; set; }
        public string? Name { get; set; }
    }
}
