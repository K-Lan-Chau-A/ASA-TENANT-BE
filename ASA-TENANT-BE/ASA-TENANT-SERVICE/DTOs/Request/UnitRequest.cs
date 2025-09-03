using System;
using System.Collections.Generic;
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
        public long? ShopId { get; set; }
        public string? Name { get; set; }
    }
}
