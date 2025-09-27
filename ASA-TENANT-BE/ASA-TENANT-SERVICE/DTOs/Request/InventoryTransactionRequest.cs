using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public IFormFile? InventoryTransImageFile { get; set; }
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
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "ShopId is required")]
        public long ShopId { get; set; }
    }
}
