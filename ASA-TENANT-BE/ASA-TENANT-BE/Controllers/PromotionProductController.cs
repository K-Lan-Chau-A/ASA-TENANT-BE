using ASA_TENANT_REPO.Models;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Implenment;
using ASA_TENANT_SERVICE.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASA_TENANT_BE.Controllers
{
    [Route("api/promotion-products")]
    [ApiController]
    public class PromotionProductController : ControllerBase
    {
        private readonly IPromotionProductService _promotionProductService;
        public PromotionProductController(IPromotionProductService promotionProductService)
        {
            _promotionProductService = promotionProductService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PromotionProduct>>> GetFiltered([FromQuery] PromotionProductGetRequest requestDto, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _promotionProductService.GetFilteredPromotionProductsAsync(requestDto, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<ActionResult<PromotionProductResponse>> Create([FromBody] PromotionProductRequest request)
        {
            try
            {
                var result = await _promotionProductService.CreateAsync(request);
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
        public async Task<ActionResult<PromotionProductResponse>> Update(long id, [FromBody] PromotionProductRequest request)
        {
            try
            {
                var result = await _promotionProductService.UpdateAsync(id, request);
                if (!result.Success)
                {
                    if (string.Equals(result.Message, "PromotionProduct not found", StringComparison.OrdinalIgnoreCase))
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
        public async Task<ActionResult<bool>> Delete(long id)
        {
            try
            {
                var result = await _promotionProductService.DeleteAsync(id);
                if (!result.Success || result.Data == false)
                {
                    if (string.Equals(result.Message, "PromotionProduct not found", StringComparison.OrdinalIgnoreCase))
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
