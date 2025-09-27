using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Response
{
    public class InventoryTransactionResponse
    {
        public long InventoryTransactionId { get; set; }
        public short? Type { get; set; }
        public long? ProductId { get; set; }
        public long? OrderId { get; set; }
        public long? UnitId { get; set; }
        public int? Quantity { get; set; }
        public string InventoryTransImageURL { get; set; }
        public decimal? Price { get; set; }
        public long? ShopId { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
