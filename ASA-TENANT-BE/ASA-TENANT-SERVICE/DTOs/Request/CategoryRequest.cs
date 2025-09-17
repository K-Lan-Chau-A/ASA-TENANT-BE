using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Request
{
    public class CategoryRequest
    {
        public string? categoryName { get; set; }
        public string? description { get; set; }
        public long shopId { get; set; }
    }

    public class CategoryGetRequest
    {
        public long? categoryId { get; set; }
        public string? categoryName { get; set; }
        public long shopId { get; set; }
    }
}
