using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Response
{
    public class CategoryResponse
    {
        public long categoryId { get; set; }
        public string categoryName { get; set; }
        public string description { get; set; }
    }
}
