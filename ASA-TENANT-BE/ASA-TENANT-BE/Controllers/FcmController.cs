using ASA_TENANT_REPO.Models;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Implenment;
using ASA_TENANT_SERVICE.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASA_TENANT_BE.Controllers
{
    [Route("api/Fcm")]
    [ApiController]
    public class FcmController : ControllerBase
    {
        private readonly IFcmService _fcmService;
        public FcmController(IFcmService fcmService)
        {
            _fcmService = fcmService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FcmResponse>>> GetFiltered([FromQuery] FcmGetRequest requestDto, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _fcmService.GetFilteredFcmAsync(requestDto, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<ActionResult<FcmResponse>> Create([FromBody] FcmRequest request)
        {
            var result = await _fcmService.CreateOrActiveAsync(request);
            return Ok(result);
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<FcmResponse>> Update(long id, [FromBody] FcmRequest request)
        {
            var result = await _fcmService.UpdateAsync(id, request);
            return Ok(result);
        }
        [HttpPut]
        public async Task<IActionResult> RefreshToken([FromBody] FcmRefreshTokenRequest request)
        {
            var result = await _fcmService.RefreshDeviceTokenAsync(request);
            if (!result)
                return NotFound(new { message = "Token record not found." });
            return Ok(new { message = "Device token refreshed." });
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> Delete(long id)
        {
            var result = await _fcmService.DeleteAsync(id);
            return Ok(result);
        }
        [HttpDelete]
        public async Task<IActionResult> LogoutDevice([FromBody] FcmRequest request)
        {
            await _fcmService.LogoutDeviceAsync(request);
            return Ok(new { message = "Device token deactivated." });
        }
    }
}
