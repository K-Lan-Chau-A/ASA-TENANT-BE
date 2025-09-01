using ASA_TENANT_REPO.Models;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Implenment;
using ASA_TENANT_SERVICE.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASA_TENANT_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogActivityController : ControllerBase
    {
        private readonly ILogActivityService _logActivityService;
        public LogActivityController(ILogActivityService logActivityService)
        {
            _logActivityService = logActivityService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LogActivity>>> GetFiltered([FromQuery] LogActivityGetRequest requestDto, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _logActivityService.GetFilteredLogActivitiesAsync(requestDto, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<ActionResult<LogActivityResponse>> Create([FromBody] LogActivityRequest request)
        {
            var result = await _logActivityService.CreateAsync(request);
            return Ok(result);
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<LogActivityResponse>> Update(long id, [FromBody] LogActivityRequest request)
        {
            var result = await _logActivityService.UpdateAsync(id, request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> Delete(long id)
        {
            var result = await _logActivityService.DeleteAsync(id);
            return Ok(result);
        }

    }
}
