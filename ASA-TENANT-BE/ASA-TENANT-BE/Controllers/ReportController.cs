using ASA_TENANT_REPO.Models;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.Implenment;
using ASA_TENANT_SERVICE.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASA_TENANT_BE.Controllers
{
    [Route("api/reports")]
    [ApiController]
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
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
