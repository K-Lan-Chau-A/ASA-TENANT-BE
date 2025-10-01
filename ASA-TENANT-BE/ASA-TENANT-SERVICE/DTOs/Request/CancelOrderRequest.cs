using System.ComponentModel.DataAnnotations;

namespace ASA_TENANT_SERVICE.DTOs.Request
{
    public class CancelOrderRequest
    {
        [StringLength(500, ErrorMessage = "Lý do hủy đơn hàng không được vượt quá 500 ký tự")]
        public string? Reason { get; set; }
    }
}
