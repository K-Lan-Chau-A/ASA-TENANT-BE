using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.DTOs.Request
{
    public class ProductRequest
    {
        // Thông tin chung của sản phẩm
        public long ShopId { get; set; }    
        public string ProductName { get; set; }   
        public string Barcode { get; set; }      

        public long? CategoryId { get; set; }
        public string ImageUrl { get; set; }
        public decimal? Price { get; set; }
        public decimal? Discount { get; set; }
        public short? Status { get; set; }

        public List<UnitProductRequest> Units { get; set; } = new List<UnitProductRequest>();

        public InventoryTransactionProductRequest InventoryTransaction { get; set; }
    }

    public class UnitProductRequest
    {
        public string Name { get; set; }                   // VD: "Lon", "Lốc", "Thùng"
        public decimal ConversionFactor { get; set; }      // VD: 1 (lon), 6 (lốc), 24 (thùng)
        public decimal Price { get; set; }                 // Giá bán theo đơn vị này
        public bool IsBaseUnit { get; set; }               // Đánh dấu đơn vị gốc
    }

    public class InventoryTransactionProductRequest
    {
        public int Quantity { get; set; }                  // Số lượng nhập
        public decimal? Price { get; set; }                // Giá bán mới (nếu muốn cập nhật)
        public string ImageUrl { get; set; }               // Link hóa đơn/ảnh chứng từ
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
