using ASA_TENANT_REPO.Models;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Implenment;
using ASA_TENANT_SERVICE.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASA_TENANT_BE.Controllers
{
    [Route("api/nfcs")]
    [ApiController]
    public class NfcController : ControllerBase
    {
        private readonly INfcService _nfcService;
        public NfcController(INfcService nfcService)
        {
            _nfcService = nfcService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Nfc>>> GetFiltered([FromQuery] NfcGetRequest requestDto, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _nfcService.GetFilteredNfcsAsync(requestDto, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<ActionResult<NfcResponse>> Create([FromBody] NfcRequest request)
        {
            var result = await _nfcService.CreateAsync(request);
            return Ok(result);
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<NfcResponse>> Update(long id, [FromBody] NfcRequest request)
        {
            var result = await _nfcService.UpdateAsync(id, request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> Delete(long id)
        {
            var result = await _nfcService.DeleteAsync(id);
            return Ok(result);
        }

    }
}
