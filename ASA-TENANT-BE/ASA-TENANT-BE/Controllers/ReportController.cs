using ASA_TENANT_REPO.Models;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Implenment;
using ASA_TENANT_SERVICE.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using ASA_TENANT_BE.CustomAttribute;
using Microsoft.AspNetCore.Mvc;

namespace ASA_TENANT_BE.Controllers
{
    [Route("api/reports")]
    [ApiController]
    [Authorize]
    [RequireFeature(1)] // Xuất báo cáo
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Report>>> GetFiltered([FromQuery] ReportGetRequest requestDto, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _reportService.GetFilteredReportAsync(requestDto, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPost("professional-revenue")]
        public async Task<IActionResult> GenerateProfessionalRevenueReport([FromBody] ExcelReportRequest request)
        {
            try
            {
                var excelBytes = await _reportService.GenerateProfessionalRevenueReportAsync(request);

                var fileName = $"Professional_Revenue_Report_{request.StartDate:yyyyMMdd}_{request.EndDate:yyyyMMdd}.xlsx";

                return File(excelBytes,
                           "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                           fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("statistics-overview")]
        public async Task<ActionResult<StatisticsOverviewResponse>> GetStatisticsOverview([FromQuery] long shopId)
        {
            try
            {
                if (shopId <= 0)
                {
                    return BadRequest("ShopId must be greater than 0");
                }

                var result = await _reportService.GetStatisticsOverviewAsync(shopId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("shift-close-report")]
        public async Task<ActionResult<ShiftCloseReportResponse>> GetShiftCloseReport([FromQuery] long shiftId)
        {
            try
            {
                if (shiftId <= 0)
                {
                    return BadRequest("shiftId must be greater than 0");
                }

                var result = await _reportService.GenerateShiftCloseReportAsync(shiftId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Dùng 404 nếu ca không tồn tại, 400 nếu ca chưa đóng, còn lại 500
                var message = ex.Message ?? string.Empty;
                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(new { message });
                }
                if (message.Contains("chưa được đóng", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message });
                }
                return StatusCode(500, new { message });
            }
        }

    }
        
}
