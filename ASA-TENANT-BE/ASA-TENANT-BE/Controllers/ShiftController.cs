using ASA_TENANT_REPO.Models;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASA_TENANT_BE.Controllers
{
    [Route("api/shifts")]
    [ApiController]
    public class ShiftController : ControllerBase
    {
        private readonly IShiftService _shiftService;
        public ShiftController(IShiftService shiftService)
        {
            _shiftService = shiftService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Shift>>> GetFiltered([FromQuery] ShiftGetRequest requestDto, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _shiftService.GetFilteredCategoriesAsync(requestDto, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<ActionResult<ShiftResponse>> Create([FromBody] ShiftRequest request)
        {
            var result = await _shiftService.CreateAsync(request);
            return Ok(result);
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<ShiftResponse>> Update(long id, [FromBody] ShiftRequest request)
        {
            var result = await _shiftService.UpdateAsync(id, request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> Delete(long id)
        {
            var result = await _shiftService.DeleteAsync(id);
            return Ok(result);
        }
        [HttpPost("open-shift")]
        public async Task<ActionResult<ShiftResponse>> OpenShift([FromBody] ShiftOpenRequest shiftOpenRequest)
        {
            var result = await _shiftService.OpenShift(shiftOpenRequest);
            return Ok(result);
        }
        [HttpPost("close-shift")]
        public async Task<ActionResult<ShiftResponse>> CloseShift([FromBody] ShiftCloseRequest shiftCloseRequest)
        {
            var result = await _shiftService.CloseShift(shiftCloseRequest);
            return Ok(result);
        }
    }
}
