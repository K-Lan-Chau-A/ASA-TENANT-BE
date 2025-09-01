using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Request
{
    public class InventoryTransactionRequest
    {
        public int? Type { get; set; }
        public long ProductId { get; set; }
        public long? OrderId { get; set; }
        public long UnitId { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
        public decimal? Price { get; set; }
        public long ShopId { get; set; }
    }

    public class InventoryTransactionGetRequest
    {
        public long InventoryTransactionId { get; set; }
        public int? Type { get; set; }
        public long ProductId { get; set; }
        public long? OrderId { get; set; }
        public long UnitId { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? Price { get; set; }
        public long ShopId { get; set; }
    }
}
