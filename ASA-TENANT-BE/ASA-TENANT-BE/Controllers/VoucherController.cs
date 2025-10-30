using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ASA_TENANT_BE.CustomAttribute;

namespace ASA_TENANT_BE.Controllers
{
    [Route("api/vouchers")]
    [ApiController]
    [Authorize]
    [RequireFeature(6)] // Quản lí voucher
    public class VoucherController : ControllerBase
    {
        private readonly IVoucherService _voucherService;
        public VoucherController(IVoucherService voucherService)
        {
            _voucherService = voucherService;
        }

        [HttpGet]
        public async Task<ActionResult> GetFiltered([FromQuery] VoucherGetRequest requestDto, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _voucherService.GetFilteredVouchersAsync(requestDto, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<VoucherResponse>> Create([FromBody] VoucherRequest request)
        {
            try
            {
                var result = await _voucherService.CreateAsync(request);
                if (!result.Success || result.Data == null)
                {
                    return BadRequest(result);
                }
                return StatusCode(201, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<VoucherResponse>> Update(long id, [FromBody] VoucherRequest request)
        {
            try
            {
                var result = await _voucherService.UpdateAsync(id, request);
                if (!result.Success)
                {
                    if (string.Equals(result.Message, "Voucher not found", StringComparison.OrdinalIgnoreCase))
                    {
                        return NotFound(result);
                    }
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "1")] // Admin only
        public async Task<ActionResult<bool>> Delete(long id)
        {
            try
            {
                var result = await _voucherService.DeleteAsync(id);
                if (!result.Success || result.Data == false)
                {
                    if (string.Equals(result.Message, "Voucher not found", StringComparison.OrdinalIgnoreCase))
                    {
                        return NotFound(result);
                    }
                    return BadRequest(result);
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}


