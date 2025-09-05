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
        public async Task<ActionResult<IEnumerable<Fcm>>> GetFiltered([FromQuery] FcmGetRequest requestDto, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
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
            var result = await _fcmService.CreateAsync(request);
            return Ok(result);
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<FcmResponse>> Update(long id, [FromBody] FcmRequest request)
        {
            var result = await _fcmService.UpdateAsync(id, request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> Delete(long id)
        {
            var result = await _fcmService.DeleteAsync(id);
            return Ok(result);
        }
    }
}
