using ASA_TENANT_REPO.Models;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASA_TENANT_BE.Controllers
{
    [Route("api/shops")]
    [ApiController]
    public class ShopController : ControllerBase
    {
        private readonly IShopService _shopService;

        public ShopController(IShopService shopService)
        {
            _shopService = shopService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Shop>>> GetFiltered([FromQuery] ShopGetRequest requestDto, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _shopService.GetFilteredShopsAsync(requestDto, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<ActionResult<ShopResponse>> Create([FromBody] ShopRequest request)
        {
            var result = await _shopService.CreateAsync(request);
            return Ok(result);
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<ShopResponse>> Update(long id, [FromBody] ShopRequest request)
        {
            var result = await _shopService.UpdateAsync(id, request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> Delete(long id)
        {
            var result = await _shopService.DeleteAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Cập nhật Sepay API key cho shop
        /// </summary>
        [HttpPut("{id}/sepay-api-key")]
        public async Task<ActionResult> UpdateSepayApiKey(long id, [FromBody] SepayApiKeyRequest request)
        {
            try
            {
                var result = await _shopService.UpdateSepayApiKeyAsync(id, request.ApiKey);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Test Sepay API key
        /// </summary>
        [HttpPost("test-sepay-api-key")]
        public async Task<ActionResult> TestSepayApiKey([FromBody] SepayApiKeyRequest request)
        {
            try
            {
                var result = await _shopService.TestSepayApiKeyAsync(request.ApiKey);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }

    public class SepayApiKeyRequest
    {
        public string ApiKey { get; set; } = string.Empty;
    }
}
