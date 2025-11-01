using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Interface;
using Microsoft.AspNetCore.Mvc;

namespace ASA_TENANT_BE.Controllers
{
    [Route("api/shop-subscriptions")]
    [ApiController]
    public class ShopSubscriptionController : ControllerBase
    {
        private readonly IShopSubscriptionService _shopSubscriptionService;
        public ShopSubscriptionController(IShopSubscriptionService shopSubscriptionService)
        {
            _shopSubscriptionService = shopSubscriptionService;
        }

        [HttpGet]
        public async Task<ActionResult> GetFiltered([FromQuery] ShopSubscriptionGetRequest requestDto, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _shopSubscriptionService.GetFilteredAsync(requestDto, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<ShopSubscriptionResponse>> Create([FromBody] ShopSubscriptionRequest request)
        {
            try
            {
                var result = await _shopSubscriptionService.CreateAsync(request);
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
        public async Task<ActionResult<ShopSubscriptionResponse>> Update(long id, [FromBody] ShopSubscriptionRequest request)
        {
            try
            {
                var result = await _shopSubscriptionService.UpdateAsync(id, request);
                if (!result.Success)
                {
                    if (string.Equals(result.Message, "ShopSubscription not found", StringComparison.OrdinalIgnoreCase))
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
                var result = await _shopSubscriptionService.DeleteAsync(id);
                if (!result.Success || result.Data == false)
                {
                    if (string.Equals(result.Message, "ShopSubscription not found", StringComparison.OrdinalIgnoreCase))
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


