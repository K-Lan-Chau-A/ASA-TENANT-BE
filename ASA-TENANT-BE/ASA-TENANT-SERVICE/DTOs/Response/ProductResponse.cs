using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Response
{
    public class ProductResponse
    {
        public long ProductId { get; set; }
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
        public string? CategoryName { get; set; }
        public long? UnitIdFk { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
}
