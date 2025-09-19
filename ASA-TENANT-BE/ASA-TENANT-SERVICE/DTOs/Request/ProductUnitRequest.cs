using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Request
{
    public class ProductUnitRequest
    {
        public long ShopId { get; set; }
        public long UnitId { get; set; }
        public decimal? ConversionFactor { get; set; }
        public decimal Price { get; set; }
        public long ProductId { get; set; }
    }

    public class ProductUnitGetRequest
    {
        public long? ProductUnitId { get; set; }
        [Required(ErrorMessage = "ShopId is required")]
        public long ShopId { get; set; }
        public long? UnitId { get; set; }
        public decimal? ConversionFactor { get; set; }
        public decimal? Price { get; set; }
        public long? ProductId { get; set; }
    }
}
