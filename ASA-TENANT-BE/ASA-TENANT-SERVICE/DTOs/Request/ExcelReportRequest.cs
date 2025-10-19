using System.ComponentModel.DataAnnotations;

namespace ASA_TENANT_SERVICE.DTOs.Request
{
    public class ExcelReportRequest
    {
        [Required(ErrorMessage = "StartDate is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "EndDate is required")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "ShopId is required")]
        public long ShopId { get; set; }
    }
}
