using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Response
{
    public class ProductUnitResponse
    {
        public long ProductUnitId { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public long UnitId { get; set; }
        public string UnitName { get; set; }
        public double ConversionFactor { get; set; }
        public double Price { get; set; }
        public long ShopId { get; set; }
    }
}
