using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Request
{
    public class ProductRequest
    {
        public string ProductName { get; set; }
        public int? Quantity { get; set; }
        public decimal? Cost { get; set; }
        public decimal? Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? Barcode { get; set; }
        public decimal? Discount { get; set; }
        public bool? IsLow { get; set; }
        public short? Status { get; set; }

        public long ShopId { get; set; }          
        public long CategoryId { get; set; }     
        public long? UnitIdFk { get; set; }      
    }

    public class ProductGetRequest
    {
        public long? ProductId { get; set; }
        public long? CategoryId { get; set; }
        public long? ShopId { get; set; }
        public long? UnitIdFk { get; set; }
        public string? ProductName { get; set; }
        public short? Status { get; set; }
        public decimal? Price { get; set; }
        public decimal? Cost { get; set; }
        public decimal? Discount { get; set; }
        public string? Barcode { get; set; }
        public int? Quantity { get; set; }

    }
}
